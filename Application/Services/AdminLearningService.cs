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
                LearningHelper.ApplySentenceConfig(existingLink, sentenceRequest.Position, sentenceRequest.BlankWord, sentenceRequest.Hint, sentenceRequest.AnswerList);
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

    public async Task<LearningAdminCardSentenceConfigResponse> AttachSentenceAsync(
        string cardId,
        AttachLearningCardSentenceRequest request)
    {
        await EnsureCardExistsAsync(cardId);
        await EnsureSentenceExistsAsync(request.SentenceId);

        var existingLink = await _unitOfWork.CardSentences.GetByCardAndSentenceIdAsync(cardId, request.SentenceId);
        if (existingLink != null)
            throw new AppException(MessageConstants.LearningMessage.SENTENCE_ALREADY_ATTACHED, 400);

        var link = new CardSentence
        {
            CardId = cardId,
            SentenceId = request.SentenceId,
            Position = request.Position,
            BlankWord = StringHelper.NormalizeOptional(request.BlankWord),
            Hint = StringHelper.NormalizeOptional(request.Hint),
            AnswerList = StringHelper.NormalizeAnswerList(request.AnswerList, request.BlankWord),
        };

        await _unitOfWork.CardSentences.AddAsync(link);
        await _unitOfWork.SaveChangesAsync();

        var updatedCard = await GetLearningCardRequiredAsync(cardId);
        var updatedLink = updatedCard.CardSentences.FirstOrDefault(x => x.SentenceId == request.SentenceId)
            ?? throw new AppException(MessageConstants.LearningMessage.SENTENCE_NOT_ATTACHED, 404);

        return updatedLink.ToAdminSentenceConfigResponse();
    }

    public async Task<LearningAdminCardSentenceConfigResponse> UpdateSentenceConfigAsync(
        string cardId,
        string sentenceId,
        UpdateLearningCardSentenceConfigRequest request)
    {
        await EnsureCardExistsAsync(cardId);

        var link = await _unitOfWork.CardSentences.GetByCardAndSentenceIdAsync(cardId, sentenceId)
            ?? throw new AppException(MessageConstants.LearningMessage.SENTENCE_NOT_ATTACHED, 404);

        LearningHelper.ApplySentenceConfig(link, request.Position, request.BlankWord, request.Hint, request.AnswerList);
        _unitOfWork.CardSentences.UpdateAsync(link);
        await _unitOfWork.SaveChangesAsync();

        var updatedCard = await GetLearningCardRequiredAsync(cardId);
        var updatedLink = updatedCard.CardSentences.FirstOrDefault(x => x.SentenceId == sentenceId)
            ?? throw new AppException(MessageConstants.LearningMessage.SENTENCE_NOT_ATTACHED, 404);

        return updatedLink.ToAdminSentenceConfigResponse();
    }

    public async Task<bool> DeleteSentenceAsync(string cardId, string sentenceId)
    {
        await EnsureCardExistsAsync(cardId);

        var link = await _unitOfWork.CardSentences.GetByCardAndSentenceIdAsync(cardId, sentenceId)
            ?? throw new AppException(MessageConstants.LearningMessage.SENTENCE_NOT_ATTACHED, 404);

        _unitOfWork.CardSentences.DeleteAsync(link);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<List<LearningAdminCardSentenceConfigResponse>> ReorderSentencesAsync(
        string cardId,
        ReorderLearningCardSentencesRequest request)
    {
        await EnsureCardExistsAsync(cardId);

        var existingLinks = await _unitOfWork.CardSentences.GetByCardIdAsync(cardId);
        var linkMap = existingLinks.ToDictionary(x => x.SentenceId, StringComparer.Ordinal);

        foreach (var item in request.Items)
        {
            if (!linkMap.TryGetValue(item.SentenceId, out var link))
                throw new AppException(MessageConstants.LearningMessage.SENTENCE_NOT_ATTACHED, 404);

            link.Position = item.Position;
            _unitOfWork.CardSentences.UpdateAsync(link);
        }

        await _unitOfWork.SaveChangesAsync();

        var updatedCard = await GetLearningCardRequiredAsync(cardId);
        return updatedCard.CardSentences
            .Where(cs => cs.Sentence != null)
            .OrderBy(cs => cs.Position)
            .Select(cs => cs.ToAdminSentenceConfigResponse())
            .ToList();
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
                var allIssues = LearningAdminHelper.BuildIssues(card);
                var availableModes = LearningAdminHelper.BuildAvailableModes(card, allIssues);

                var filteredIssues = LearningAdminHelper.FilterIssuesByMode(allIssues, parsedMode);

                if (parsedIssueType.HasValue)
                {
                    filteredIssues = filteredIssues
                        .Where(issue => issue.Type == parsedIssueType.Value.ToString())
                        .ToList();
                }

                return new
                {
                    Card = card,
                    FilteredIssues = filteredIssues,
                    AvailableModes = availableModes,
                };
            })
            .Where(x => x.FilteredIssues.Count > 0)
            .Select(x => x.Card.ToAdminCardIssueResponse(x.FilteredIssues, x.AvailableModes))
            .ToList();

        var (normalizedPage, normalizedPageSize) = PagingHelper.Normalize(query.Page, query.PageSize);
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

    public async Task<LearningAdminOverviewResponse> GetOverviewAsync()
    {
        var now = DateTime.UtcNow;
        var todayStartUtc = now.Date;
        var sessionsToday = await _unitOfWork.StudySessions.GetCreatedSinceAsync(todayStartUtc);
        var allDue = await _unitOfWork.UserCardProgresses.GetAllDueAsync(now);
        var totalAttempts = sessionsToday.Sum(x => x.CorrectCount + x.IncorrectCount);
        var totalCorrect = sessionsToday.Sum(x => x.CorrectCount);

        return new LearningAdminOverviewResponse
        {
            ActiveUsersToday = sessionsToday.Select(x => x.UserId).Distinct(StringComparer.Ordinal).Count(),
            SessionsToday = sessionsToday.Count,
            CompletedSessionsToday = sessionsToday.Count(x => x.CompletedAt.HasValue),
            SubmissionsToday = totalAttempts,
            DueCardsNow = allDue.Count,
            AverageAccuracy = LearningHelper.CalculateAccuracy(totalCorrect, totalAttempts),
        };
    }

    public async Task<DeckLearningAnalyticsResponse> GetDeckAnalyticsAsync(string deckId)
    {
        var deck = await _unitOfWork.Decks.GetAdminDetailByIdAsync(deckId)
            ?? throw new AppException(MessageConstants.DeckMessage.NOT_FOUND, 404);

        var cardIds = deck.Folders
            .SelectMany(folder => folder.FolderCards)
            .Select(folderCard => folderCard.CardId)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var sessions = await _unitOfWork.StudySessions.GetByDeckIdAsync(deckId);
        var progresses = cardIds.Count == 0
            ? new List<UserCardProgress>()
            : await _unitOfWork.UserCardProgresses.GetByCardIdsAsync(cardIds);

        var submissionCount = sessions.Sum(x => x.CorrectCount + x.IncorrectCount);
        var correctCount = sessions.Sum(x => x.CorrectCount);

        return new DeckLearningAnalyticsResponse
        {
            DeckId = deck.Id,
            DeckTitle = deck.Title,
            SessionCount = sessions.Count,
            CompletedSessionCount = sessions.Count(x => x.CompletedAt.HasValue),
            SubmissionCount = submissionCount,
            AverageAccuracy = LearningHelper.CalculateAccuracy(correctCount, submissionCount),
            TrackedCards = progresses.Select(x => x.CardId).Distinct(StringComparer.Ordinal).Count(),
            MasteredCards = progresses.Where(LearningHelper.IsMastered)
                .Select(x => x.CardId)
                .Distinct(StringComparer.Ordinal)
                .Count(),
            DueCards = progresses.Where(x => LearningHelper.IsDue(x, DateTime.UtcNow))
                .Select(x => x.CardId)
                .Distinct(StringComparer.Ordinal)
                .Count(),
            ModeBreakdown = sessions
                .GroupBy(x => x.Mode)
                .OrderBy(x => x.Key.ToString())
                .Select(group =>
                {
                    var modeSubmissionCount = group.Sum(x => x.CorrectCount + x.IncorrectCount);
                    var modeCorrectCount = group.Sum(x => x.CorrectCount);

                    return new DeckLearningModeAnalyticsResponse
                    {
                        Mode = group.Key.ToString(),
                        SessionCount = group.Count(),
                        CompletedSessionCount = group.Count(x => x.CompletedAt.HasValue),
                        SubmissionCount = modeSubmissionCount,
                        AverageAccuracy = LearningHelper.CalculateAccuracy(modeCorrectCount, modeSubmissionCount),
                    };
                })
                .ToList(),
        };
    }

    public async Task<CardLearningAnalyticsResponse> GetCardAnalyticsAsync(string cardId)
    {
        var card = await GetLearningCardRequiredAsync(cardId);
        var sessions = await _unitOfWork.StudySessions.GetByCardIdAsync(cardId);
        var progresses = await _unitOfWork.UserCardProgresses.GetByCardIdAsync(cardId);
        var decks = await _unitOfWork.Decks.GetAdminDecksContainingCardIdsAsync(new List<string> { cardId });

        return new CardLearningAnalyticsResponse
        {
            CardId = card.Id,
            CardType = card.CardType.ToString(),
            Title = card.Title,
            Summary = card.Summary,
            IncludedSessionCount = sessions.Count,
            IncludedCompletedSessionCount = sessions.Count(x => x.CompletedAt.HasValue),
            TrackedUsers = progresses.Select(x => x.UserId).Distinct(StringComparer.Ordinal).Count(),
            MasteredUsers = progresses.Count(LearningHelper.IsMastered),
            DueUsers = progresses.Count(x => LearningHelper.IsDue(x, DateTime.UtcNow)),
            AverageSrsLevel = LearningHelper.CalculateAverageSrsLevel(progresses),
            AverageConsecutiveCorrect = LearningHelper.CalculateAverageConsecutiveCorrect(progresses),
            LastReviewedAt = progresses
                .Where(x => x.LastReviewedAt.HasValue)
                .OrderByDescending(x => x.LastReviewedAt)
                .Select(x => x.LastReviewedAt)
                .FirstOrDefault(),
            SrsDistribution = progresses
                .GroupBy(x => x.SrsLevel)
                .OrderBy(x => x.Key)
                .Select(group => new CardLearningSrsDistributionResponse
                {
                    SrsLevel = group.Key.ToString(),
                    UserCount = group.Count(),
                })
                .ToList(),
            Decks = decks
                .OrderBy(x => x.Title)
                .Select(x => new CardLearningDeckUsageResponse
                {
                    DeckId = x.Id,
                    DeckTitle = x.Title,
                })
                .ToList(),
        };
    }

    public async Task<UserLearningProgressResponse> GetUserProgressAsync(string userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId)
            ?? throw new AppException(MessageConstants.CommonMessage.NOT_FOUND, 404);

        var progresses = await _unitOfWork.UserCardProgresses.GetByUserIdAsync(userId);
        var recentSessions = await _unitOfWork.StudySessions.GetRecentByUserAsync(userId, 20);
        var trackedCardIds = progresses
            .Select(x => x.CardId)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var decks = trackedCardIds.Count == 0
            ? new List<Deck>()
            : await _unitOfWork.Decks.GetAdminDecksContainingCardIdsAsync(trackedCardIds);

        return new UserLearningProgressResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            TotalTrackedCards = trackedCardIds.Count,
            MasteredCards = progresses
                .Where(LearningHelper.IsMastered)
                .Select(x => x.CardId)
                .Distinct(StringComparer.Ordinal)
                .Count(),
            DueCards = progresses
                .Where(x => LearningHelper.IsDue(x, DateTime.UtcNow))
                .Select(x => x.CardId)
                .Distinct(StringComparer.Ordinal)
                .Count(),
            AverageSrsLevel = LearningHelper.CalculateAverageSrsLevel(progresses),
            AverageConsecutiveCorrect = LearningHelper.CalculateAverageConsecutiveCorrect(progresses),
            LastReviewedAt = progresses
                .Where(x => x.LastReviewedAt.HasValue)
                .OrderByDescending(x => x.LastReviewedAt)
                .Select(x => x.LastReviewedAt)
                .FirstOrDefault(),
            RecentSessionCount = recentSessions.Count,
            SrsDistribution = progresses
                .GroupBy(x => x.SrsLevel)
                .OrderBy(x => x.Key)
                .Select(group => new UserLearningSrsDistributionResponse
                {
                    SrsLevel = group.Key.ToString(),
                    CardCount = group.Count(),
                })
                .ToList(),
            Decks = decks
                .OrderBy(x => x.Title)
                .Select(deck =>
                {
                    var deckCardIds = deck.Folders
                        .SelectMany(folder => folder.FolderCards)
                        .Select(folderCard => folderCard.CardId)
                        .Distinct(StringComparer.Ordinal)
                        .ToList();

                    var deckProgresses = progresses
                        .Where(progress => deckCardIds.Contains(progress.CardId, StringComparer.Ordinal))
                        .ToList();

                    return new UserLearningDeckProgressResponse
                    {
                        DeckId = deck.Id,
                        DeckTitle = deck.Title,
                        TrackedCards = deckProgresses
                            .Select(x => x.CardId)
                            .Distinct(StringComparer.Ordinal)
                            .Count(),
                        MasteredCards = deckProgresses
                            .Where(LearningHelper.IsMastered)
                            .Select(x => x.CardId)
                            .Distinct(StringComparer.Ordinal)
                            .Count(),
                        DueCards = deckProgresses
                            .Where(x => LearningHelper.IsDue(x, DateTime.UtcNow))
                            .Select(x => x.CardId)
                            .Distinct(StringComparer.Ordinal)
                            .Count(),
                    };
                })
                .Where(x => x.TrackedCards > 0)
                .ToList(),
        };
    }

    public async Task<LearningPreviewResponse> PreviewCardAsync(string cardId, LearningPreviewQuery query)
    {
        var card = await GetLearningCardRequiredAsync(cardId);
        var mode = LearningHelper.ParseStudyMode(query.Mode);
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
        var selectedSentence = LearningModeEligibilityHelper.GetFirstFillInBlankSentence(card);

        if (selectedSentence != null)
        {
            var acceptedAnswers = StringHelper.NormalizeAnswerList(selectedSentence.AnswerList, selectedSentence.BlankWord);
            var blankValue = selectedSentence.BlankWord ?? acceptedAnswers.FirstOrDefault() ?? string.Empty;

            return new LearningPreviewResponse
            {
                CardId = card.Id,
                Mode = StudyMode.FillInBlank.ToString(),
                Prompt = "Điền vào chỗ trống",
                QuestionText = LearningHelper.ReplaceFirstBlank(selectedSentence.Sentence!.Text, blankValue),
                SecondaryText = selectedSentence.Sentence.Meaning,
                Hint = selectedSentence.Hint,
                SentenceId = selectedSentence.SentenceId,
                QuestionSource = "Sentence",
                AcceptedAnswerCount = acceptedAnswers.Count,
                AllowsMultipleSelection = acceptedAnswers.Count > 1,
                Warnings = warnings,
            };
        }

        if (card.CardType != CardType.Kanji)
        {
            warnings.Add("Card is not ready for fill-in-blank. Attach a valid sentence and configure blankWord or answerList.");

            return new LearningPreviewResponse
            {
                CardId = card.Id,
                Mode = StudyMode.FillInBlank.ToString(),
                Prompt = "Điền vào chỗ trống",
                QuestionSource = "Sentence",
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
            QuestionSource = "CardPrompt",
            AcceptedAnswerCount = fallbackAnswers.Count,
            AllowsMultipleSelection = fallbackAnswers.Count > 1,
            Warnings = warnings,
        };
    }

    private async Task<LearningPreviewResponse> BuildMultipleChoicePreviewAsync(Card card, LearningPreviewQuery query, List<string> warnings)
    {
        var questionType = LearningHelper.ResolveEnumSetting(query.MultipleChoiceQuestion, MultipleChoiceQuestionType.TitleToSummary);
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
            QuestionSource = "CardPrompt",
            AcceptedAnswerCount = string.IsNullOrWhiteSpace(acceptedAnswer) ? 0 : 1,
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
        var front = LearningHelper.ResolveEnumSetting(query.FlashcardFront, FlashcardContentType.Title);
        var back = LearningHelper.ResolveEnumSetting(query.FlashcardBack, FlashcardContentType.Summary);
        var frontText = LearningHelper.ResolveFlashcardContent(card, front);
        var backText = LearningHelper.ResolveFlashcardContent(card, back);

        if (string.IsNullOrWhiteSpace(frontText) || string.IsNullOrWhiteSpace(backText))
            warnings.Add("One side of the flashcard is empty. End-user flashcard settings should avoid this combination.");

        return new LearningPreviewResponse
        {
            CardId = card.Id,
            Mode = StudyMode.Flashcard.ToString(),
            Prompt = "Xem flashcard rồi đánh dấu đang học hoặc đã biết",
            FrontText = frontText,
            BackText = backText,
            QuestionSource = "CardPrompt",
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
}
