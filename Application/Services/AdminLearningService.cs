using Application.Common;
using Application.DTOs.LearningAdmin;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class AdminLearningService : IAdminLearningService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Random _random = new();

    public AdminLearningService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<LearningAdminCardConfigResponse> GetCardConfigAsync(string cardId)
    {
        var card = await GetLearningCardRequiredAsync(cardId);
        var issues = LearningAdminHelper.BuildIssues(card);
        return card.ToAdminCardConfigResponse(issues);
    }

    public async Task<LearningAdminCardConfigResponse> UpdateCardConfigAsync(string cardId, UpdateLearningCardConfigRequest request)
    {
        var card = await _unitOfWork.Cards.GetByIdAsync(cardId)
            ?? throw new AppException(MessageConstants.LearningMessage.CARD_NOT_FOUND, 404);

        card.Summary = request.Summary.Trim();
        card.UpdatedAt = DateTime.UtcNow;

        var existingLinks = await _unitOfWork.CardSentences.GetByCardIdAsync(cardId);
        var keptSentenceIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var sentenceRequest in request.Sentences)
        {
            await EnsureSentenceExistsAsync(sentenceRequest.SentenceId);

            if (!keptSentenceIds.Add(sentenceRequest.SentenceId))
                continue;

            var existingLink = existingLinks.FirstOrDefault(x => x.SentenceId == sentenceRequest.SentenceId);
            if (existingLink != null)
            {
                ApplySentenceConfig(existingLink, sentenceRequest.Position, sentenceRequest.BlankWord, sentenceRequest.Hint, sentenceRequest.AnswerList);
                _unitOfWork.CardSentences.UpdateAsync(existingLink);
                continue;
            }

            await _unitOfWork.CardSentences.AddAsync(new CardSentence
            {
                CardId = cardId,
                SentenceId = sentenceRequest.SentenceId,
                Position = sentenceRequest.Position,
                BlankWord = StringHelper.NormalizeOptional(sentenceRequest.BlankWord),
                Hint = StringHelper.NormalizeOptional(sentenceRequest.Hint),
                AnswerList = StringHelper.NormalizeAnswerList(sentenceRequest.AnswerList, sentenceRequest.BlankWord),
            });
        }

        foreach (var staleLink in existingLinks.Where(x => !keptSentenceIds.Contains(x.SentenceId)))
        {
            _unitOfWork.CardSentences.DeleteAsync(staleLink);
        }

        _unitOfWork.Cards.UpdateAsync(card);
        await _unitOfWork.SaveChangesAsync();

        return await GetCardConfigAsync(cardId);
    }

    public async Task<LearningAdminCardSentenceConfigResponse> UpdateSentenceConfigAsync(
        string cardId,
        string sentenceId,
        UpdateLearningCardSentenceConfigRequest request)
    {
        await EnsureCardExistsAsync(cardId);

        var link = await _unitOfWork.CardSentences.GetByCardAndSentenceIdAsync(cardId, sentenceId)
            ?? throw new AppException(MessageConstants.LearningMessage.SENTENCE_NOT_ATTACHED, 404);

        ApplySentenceConfig(link, request.Position, request.BlankWord, request.Hint, request.AnswerList);
        _unitOfWork.CardSentences.UpdateAsync(link);
        await _unitOfWork.SaveChangesAsync();

        var updatedCard = await GetLearningCardRequiredAsync(cardId);
        var updatedLink = updatedCard.CardSentences.FirstOrDefault(x => x.SentenceId == sentenceId)
            ?? throw new AppException(MessageConstants.LearningMessage.SENTENCE_NOT_ATTACHED, 404);

        return updatedLink.ToAdminSentenceConfigResponse();
    }

    public async Task<(List<LearningAdminCardIssueResponse> Items, MetaData Meta)> GetCardIssuesAsync(LearningAdminCardIssuesQuery query)
    {
        var parsedCardType = EnumParsingHelper.ParseNullable<CardType>(query.CardType);
        var parsedMode = EnumParsingHelper.ParseNullable<StudyMode>(query.Mode);
        var parsedIssueType = EnumParsingHelper.ParseNullable<LearningIssueType>(query.IssueType);
        var scopedCardIds = await ResolveDeckCardScopeAsync(query.DeckId);

        var cards = await _unitOfWork.Cards.SearchLearningAdminCardsAsync(parsedCardType, query.Q, scopedCardIds);
        var items = cards
            .Select(card =>
            {
                var issues = LearningAdminHelper.BuildIssues(card);
                issues = LearningAdminHelper.FilterIssuesByMode(issues, parsedMode);

                if (parsedIssueType.HasValue)
                {
                    issues = issues
                        .Where(issue => issue.Type == parsedIssueType.Value.ToString())
                        .ToList();
                }

                return new
                {
                    Card = card,
                    Issues = issues,
                };
            })
            .Where(x => x.Issues.Count > 0)
            .Select(x => x.Card.ToAdminCardIssueResponse(x.Issues))
            .ToList();

        var normalizedPage = query.Page <= 0 ? 1 : query.Page;
        var normalizedPageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 100);
        var pagedItems = items
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToList();

        return (pagedItems, new MetaData
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            Total = items.Count,
        });
    }

    public async Task<DeckLearningCoverageResponse> GetDeckCoverageAsync(string deckId)
    {
        var deck = await _unitOfWork.Decks.GetAdminDetailByIdAsync(deckId)
            ?? throw new AppException(MessageConstants.DeckMessage.NOT_FOUND, 404);

        var cardIds = deck.Folders
            .SelectMany(folder => folder.FolderCards)
            .Select(folderCard => folderCard.CardId)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var cards = cardIds.Count == 0
            ? new List<Card>()
            : await _unitOfWork.Cards.GetLearningAdminCardsByIdsAsync(cardIds);

        var evaluations = cards
            .Select(card =>
            {
                var issues = LearningAdminHelper.BuildIssues(card);
                return new
                {
                    Card = card,
                    Issues = issues,
                    FillInBlankReady = LearningAdminHelper.IsFillInBlankReady(card, issues),
                    MultipleChoiceReady = LearningAdminHelper.IsMultipleChoiceReady(issues),
                    FlashcardReady = LearningAdminHelper.IsFlashcardReady(issues),
                };
            })
            .ToList();

        return new DeckLearningCoverageResponse
        {
            DeckId = deck.Id,
            DeckTitle = deck.Title,
            TotalCards = evaluations.Count,
            FillInBlankReadyCount = evaluations.Count(x => x.FillInBlankReady),
            MultipleChoiceReadyCount = evaluations.Count(x => x.MultipleChoiceReady),
            FlashcardReadyCount = evaluations.Count(x => x.FlashcardReady),
            IssueCount = evaluations.Count(x => x.Issues.Count > 0),
            CardsByType = evaluations
                .GroupBy(x => x.Card.CardType)
                .OrderBy(x => x.Key.ToString())
                .Select(group => new DeckLearningCoverageCardTypeResponse
                {
                    CardType = group.Key.ToString(),
                    Total = group.Count(),
                    FillInBlankReady = group.Count(x => x.FillInBlankReady),
                    MultipleChoiceReady = group.Count(x => x.MultipleChoiceReady),
                    FlashcardReady = group.Count(x => x.FlashcardReady),
                })
                .ToList(),
        };
    }

    public async Task<LearningPreviewResponse> PreviewCardAsync(string cardId, LearningPreviewQuery query)
    {
        var card = await GetLearningCardRequiredAsync(cardId);
        var mode = ParseStudyMode(query.Mode);
        var warnings = new List<string>();

        return mode switch
        {
            StudyMode.FillInBlank => BuildFillInBlankPreview(card, warnings),
            StudyMode.MultipleChoice => await BuildMultipleChoicePreviewAsync(card, query, warnings),
            StudyMode.Flashcard => BuildFlashcardPreview(card, query, warnings),
            _ => throw new AppException(MessageConstants.LearningMessage.INVALID_MODE, 400),
        };
    }

    private LearningPreviewResponse BuildFillInBlankPreview(Card card, List<string> warnings)
    {
        var selectedSentence = card.CardSentences
            .Where(cs => cs.Sentence != null)
            .OrderBy(cs => cs.Position)
            .FirstOrDefault();

        if (selectedSentence != null)
        {
            var acceptedAnswers = StringHelper.NormalizeAnswerList(selectedSentence.AnswerList, selectedSentence.BlankWord);
            if (acceptedAnswers.Count == 0)
            {
                acceptedAnswers = LearningHelper.BuildFallbackAnswers(card);
                warnings.Add("No configured answerList found. Preview is using generated fallback answers.");
            }

            var blankValue = selectedSentence.BlankWord ?? acceptedAnswers.FirstOrDefault() ?? string.Empty;

            return new LearningPreviewResponse
            {
                CardId = card.Id,
                Mode = StudyMode.FillInBlank.ToString(),
                Prompt = "Điền vào chỗ trống",
                QuestionText = LearningHelper.ReplaceFirstBlank(selectedSentence.Sentence!.Text, blankValue),
                SecondaryText = selectedSentence.Sentence.Meaning,
                Hint = selectedSentence.Hint,
                AllowsMultipleSelection = acceptedAnswers.Count > 1,
                Warnings = warnings,
            };
        }

        warnings.Add("Card has no attached sentence. Preview is using fallback fill-in content from the card itself.");
        var fallbackAnswers = LearningHelper.BuildFallbackAnswers(card);

        return new LearningPreviewResponse
        {
            CardId = card.Id,
            Mode = StudyMode.FillInBlank.ToString(),
            Prompt = "Điền đáp án phù hợp cho thẻ",
            QuestionText = card.Summary,
            SecondaryText = card.Title,
            AllowsMultipleSelection = fallbackAnswers.Count > 1,
            Warnings = warnings,
        };
    }

    private async Task<LearningPreviewResponse> BuildMultipleChoicePreviewAsync(Card card, LearningPreviewQuery query, List<string> warnings)
    {
        var questionType = ResolveEnumSetting(query.MultipleChoiceQuestion, MultipleChoiceQuestionType.TitleToSummary);
        var shuffleOptions = query.ShuffleOptions ?? true;
        var acceptedAnswer = questionType == MultipleChoiceQuestionType.SummaryToTitle
            ? card.Title
            : card.Summary;

        if (string.IsNullOrWhiteSpace(acceptedAnswer))
            warnings.Add("Card summary is empty. Multiple-choice preview may not be usable for end users.");

        var candidateCards = await _unitOfWork.Cards.SearchLearningAdminCardsAsync(card.CardType, null, null);
        var distractors = candidateCards
            .Where(x => x.Id != card.Id)
            .Select(x => questionType == MultipleChoiceQuestionType.SummaryToTitle ? x.Title : x.Summary)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(x => !string.Equals(x, acceptedAnswer, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .ToList();

        if (distractors.Count < 3)
            warnings.Add("Not enough same-type distractors were found. Preview contains fewer options than the real session may need.");

        var options = new List<string>();
        if (!string.IsNullOrWhiteSpace(acceptedAnswer))
            options.Add(acceptedAnswer);

        options.AddRange(distractors.Take(Math.Max(4 - options.Count, 0)));

        var finalOptions = shuffleOptions
            ? LearningHelper.DistinctByRandomOrder(options, _random)
            : options.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        return new LearningPreviewResponse
        {
            CardId = card.Id,
            Mode = StudyMode.MultipleChoice.ToString(),
            Prompt = questionType == MultipleChoiceQuestionType.SummaryToTitle
                ? "Chọn từ khóa đúng của nghĩa"
                : "Chọn nghĩa đúng của thẻ",
            QuestionText = questionType == MultipleChoiceQuestionType.SummaryToTitle ? card.Summary : card.Title,
            AllowsMultipleSelection = false,
            Options = finalOptions
                .Select(option => new LearningPreviewOptionResponse
                {
                    Id = option,
                    Text = option,
                    IsCorrect = string.Equals(option, acceptedAnswer, StringComparison.OrdinalIgnoreCase),
                })
                .ToList(),
            Warnings = warnings,
        };
    }

    private LearningPreviewResponse BuildFlashcardPreview(Card card, LearningPreviewQuery query, List<string> warnings)
    {
        var front = ResolveEnumSetting(query.FlashcardFront, FlashcardContentType.Title);
        var back = ResolveEnumSetting(query.FlashcardBack, FlashcardContentType.Summary);
        var frontText = ResolveFlashcardContent(card, front);
        var backText = ResolveFlashcardContent(card, back);

        if (string.IsNullOrWhiteSpace(frontText) || string.IsNullOrWhiteSpace(backText))
            warnings.Add("One side of the flashcard is empty. End-user flashcard settings should avoid this combination.");

        return new LearningPreviewResponse
        {
            CardId = card.Id,
            Mode = StudyMode.Flashcard.ToString(),
            Prompt = "Xem flashcard rồi đánh dấu đang học hoặc đã biết",
            FrontText = frontText,
            BackText = backText,
            AllowsMultipleSelection = false,
            Warnings = warnings,
        };
    }

    private async Task<Card> GetLearningCardRequiredAsync(string cardId)
    {
        return await _unitOfWork.Cards.GetLearningAdminCardByIdAsync(cardId)
            ?? throw new AppException(MessageConstants.LearningMessage.CARD_NOT_FOUND, 404);
    }

    private async Task EnsureCardExistsAsync(string cardId)
    {
        if (await _unitOfWork.Cards.GetByIdAsync(cardId) == null)
            throw new AppException(MessageConstants.LearningMessage.CARD_NOT_FOUND, 404);
    }

    private async Task EnsureSentenceExistsAsync(string sentenceId)
    {
        if (await _unitOfWork.Sentences.GetByIdAsync(sentenceId) == null)
            throw new AppException(MessageConstants.SentenceMessage.NOT_FOUND, 404);
    }

    private async Task<List<string>?> ResolveDeckCardScopeAsync(string? deckId)
    {
        if (string.IsNullOrWhiteSpace(deckId))
            return null;

        var deck = await _unitOfWork.Decks.GetAdminDetailByIdAsync(deckId)
            ?? throw new AppException(MessageConstants.DeckMessage.NOT_FOUND, 404);

        return deck.Folders
            .SelectMany(folder => folder.FolderCards)
            .Select(folderCard => folderCard.CardId)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static void ApplySentenceConfig(CardSentence link, int position, string? blankWord, string? hint, List<string> answerList)
    {
        link.Position = position;
        link.BlankWord = StringHelper.NormalizeOptional(blankWord);
        link.Hint = StringHelper.NormalizeOptional(hint);
        link.AnswerList = StringHelper.NormalizeAnswerList(answerList, blankWord);
    }

    private static StudyMode ParseStudyMode(string mode)
    {
        try
        {
            return EnumParsingHelper.ParseRequired<StudyMode>(mode);
        }
        catch (ApplicationException)
        {
            throw new AppException(MessageConstants.LearningMessage.INVALID_MODE, 400);
        }
    }

    private static TEnum ResolveEnumSetting<TEnum>(string? value, TEnum fallback)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        try
        {
            return EnumParsingHelper.ParseRequired<TEnum>(value);
        }
        catch (ApplicationException)
        {
            throw new AppException(MessageConstants.CommonMessage.INVALID, 400);
        }
    }

    private static string ResolveFlashcardContent(Card card, FlashcardContentType contentType)
    {
        return contentType switch
        {
            FlashcardContentType.Summary => card.Summary,
            _ => card.Title,
        };
    }
}
