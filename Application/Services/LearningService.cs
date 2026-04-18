using Application.Common;
using Application.DTOs.Learning;
using Application.Helper;
using Application.IRepositories;
using Application.IServices;
using Application.Mappings;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class LearningService : ILearningService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Random _random = new();

    public LearningService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StudySessionResponse> CreateSessionAsync(CreateStudySessionRequest request, string userId)
    {
        var mode = ParseStudyMode(request.Mode);
        var settings = await ResolveSessionSettingsAsync(userId, request.Settings);
        Deck? deck = null;
        List<string> cardIds;
        List<string> selectedFolderIds;

        if (!string.IsNullOrWhiteSpace(request.DeckId))
        {
            deck = await GetReadableDeckAsync(request.DeckId, userId);
            cardIds = ResolveCardScope(deck, request.CardIds);
            selectedFolderIds = ResolveSelectedFolderIds(deck, cardIds);
        }
        else
        {
            cardIds = NormalizeRequestedCardIds(request.CardIds);
            if (cardIds.Count == 0)
                throw new AppException(MessageConstants.LearningMessage.NO_CARDS_AVAILABLE, 400);

            var cards = await _unitOfWork.Cards.GetStudyCardsByIdsAsync(cardIds);
            if (cards.Count != cardIds.Count)
                throw new AppException(MessageConstants.LearningMessage.INVALID_SCOPE, 400);

            selectedFolderIds = new List<string>();
        }

        if (cardIds.Count == 0)
            throw new AppException(MessageConstants.LearningMessage.NO_CARDS_AVAILABLE, 400);

        var session = new StudySession
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            DeckId = deck?.Id,
            Mode = mode,
            FlashcardFront = settings.FlashcardFront,
            FlashcardBack = settings.FlashcardBack,
            MultipleChoiceQuestion = settings.MultipleChoiceQuestion,
            ShuffleOptions = settings.ShuffleOptions,
            SelectedFolderIds = selectedFolderIds,
            CardIds = cardIds,
        };

        await _unitOfWork.StudySessions.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return session.ToResponse(deck);
    }

    public async Task<StudySessionResponse> GetSessionAsync(string sessionId, string userId)
    {
        var session = await GetOwnedSessionAsync(sessionId, userId);
        var deck = await GetSessionDeckAsync(session, userId);
        return session.ToResponse(deck);
    }

    public async Task<bool> DeleteSessionAsync(string sessionId, string userId)
    {
        var session = await GetOwnedSessionAsync(sessionId, userId);
        _unitOfWork.StudySessions.DeleteAsync(session);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<List<StudySessionResponse>> GetHistoryAsync(StudyHistoryQuery query, string userId)
    {
        var limit = NormalizeLimit(query.Limit, 20, 100);
        var sessions = await _unitOfWork.StudySessions.GetRecentByUserAsync(userId, limit);
        var deckIds = sessions
            .Where(x => !string.IsNullOrWhiteSpace(x.DeckId))
            .Select(x => x.DeckId!)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var decks = await Task.WhenAll(deckIds.Select(deckId => GetReadableDeckAsync(deckId, userId)));
        var deckMap = decks.ToDictionary(x => x.Id, StringComparer.Ordinal);

        return sessions
            .Select(session => session.ToResponse(
                !string.IsNullOrWhiteSpace(session.DeckId) && deckMap.ContainsKey(session.DeckId)
                    ? deckMap[session.DeckId]
                    : null))
            .ToList();
    }

    public async Task<StudyQuestionResponse?> GetNextQuestionAsync(string sessionId, string userId)
    {
        var session = await GetOwnedSessionAsync(sessionId, userId);
        if (session.CompletedAt.HasValue)
            return null;

        var nextCardId = session.CardIds.FirstOrDefault(cardId => !session.CompletedCardIds.Contains(cardId, StringComparer.Ordinal));
        if (nextCardId == null)
        {
            session.CompletedAt = DateTime.UtcNow;
            session.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.StudySessions.UpdateAsync(session);
            await _unitOfWork.SaveChangesAsync();
            return null;
        }

        var card = await GetStudyCardRequiredAsync(nextCardId);
        var progress = await _unitOfWork.UserCardProgresses.GetByUserAndCardIdAsync(userId, nextCardId);
        return await BuildQuestionResponseAsync(session, card, progress);
    }

    public async Task<SubmitStudyAnswerResponse> SubmitAnswerAsync(string sessionId, SubmitStudyAnswerRequest request, string userId)
    {
        var session = await GetOwnedSessionAsync(sessionId, userId);
        if (session.CompletedAt.HasValue)
            throw new AppException(MessageConstants.LearningMessage.SESSION_COMPLETED, 400);

        if (!session.CardIds.Contains(request.CardId, StringComparer.Ordinal))
            throw new AppException(MessageConstants.LearningMessage.CARD_NOT_IN_SESSION, 400);

        if (session.CompletedCardIds.Contains(request.CardId, StringComparer.Ordinal))
            throw new AppException(MessageConstants.LearningMessage.INVALID_SUBMISSION, 400);

        var card = await GetStudyCardRequiredAsync(request.CardId);
        var progress = await _unitOfWork.UserCardProgresses.GetByUserAndCardIdAsync(userId, request.CardId);
        var isNewProgress = progress == null;
        progress ??= await CreateProgressAsync(userId, request.CardId);

        var payload = BuildAnswerPayload(card, progress, session);
        var now = DateTime.UtcNow;
        var isCorrect = EvaluateSubmission(payload, request, session.Mode);

        ApplyProgress(progress, session.Mode, request, isCorrect, now);
        if (!string.IsNullOrWhiteSpace(payload.SelectedSentenceId))
            progress.LastSentenceId = payload.SelectedSentenceId;

        progress.LastReviewedAt = now;
        progress.UpdatedAt = now;

        if (!session.CompletedCardIds.Contains(request.CardId, StringComparer.Ordinal))
            session.CompletedCardIds.Add(request.CardId);

        if (isCorrect)
            session.CorrectCount++;
        else
            session.IncorrectCount++;

        if (session.CompletedCardIds.Count >= session.CardIds.Count)
            session.CompletedAt = now;

        session.UpdatedAt = now;

        if (!isNewProgress)
            _unitOfWork.UserCardProgresses.UpdateAsync(progress);

        _unitOfWork.StudySessions.UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return new SubmitStudyAnswerResponse
        {
            IsCorrect = isCorrect,
            CardId = card.Id,
            Mode = session.Mode.ToString(),
            AcceptedAnswers = payload.AcceptedAnswers,
            SrsLevel = progress.SrsLevel.ToString(),
            NextReviewAt = progress.NextReviewAt,
            IsMastered = progress.IsMastered,
            ConsecutiveCorrect = progress.ConsecutiveCorrect,
            CompletedCards = session.CompletedCardIds.Count,
            RemainingCards = Math.Max(session.CardIds.Count - session.CompletedCardIds.Count, 0),
        };
    }

    public async Task<CardProgressResponse> GetCardProgressAsync(string cardId, string userId)
    {
        var card = await GetStudyCardRequiredAsync(cardId);
        var progress = await _unitOfWork.UserCardProgresses.GetByUserAndCardIdAsync(userId, cardId);

        if (progress == null)
        {
            return new CardProgressResponse
            {
                CardId = card.Id,
                CardType = card.CardType.ToString(),
                Title = card.Title,
                Summary = card.Summary,
                SrsLevel = SrsLevel.level_1.ToString(),
                NextReviewAt = DateTime.UtcNow,
                ConsecutiveCorrect = 0,
                IsMastered = false,
            };
        }

        return MapCardProgress(card, progress);
    }

    public async Task<StudySessionResultResponse> GetSessionResultAsync(string sessionId, string userId)
    {
        var session = await GetOwnedSessionAsync(sessionId, userId);
        var deck = await GetSessionDeckAsync(session, userId);
        var attempts = session.CorrectCount + session.IncorrectCount;

        return new StudySessionResultResponse
        {
            SessionId = session.Id,
            DeckId = session.DeckId,
            DeckTitle = deck?.Title,
            Mode = session.Mode.ToString(),
            TotalCards = session.CardIds.Count,
            CompletedCards = session.CompletedCardIds.Count,
            CorrectCount = session.CorrectCount,
            IncorrectCount = session.IncorrectCount,
            Accuracy = attempts == 0 ? 0 : Math.Round((double)session.CorrectCount / attempts * 100, 2),
            CreatedAt = session.CreatedAt,
            CompletedAt = session.CompletedAt,
            Settings = new StudySessionSettingsResponse
            {
                FlashcardFront = session.FlashcardFront.ToString(),
                FlashcardBack = session.FlashcardBack.ToString(),
                MultipleChoiceQuestion = session.MultipleChoiceQuestion.ToString(),
                ShuffleOptions = session.ShuffleOptions,
            },
        };
    }

    public async Task<StudySessionResponse> RestartSessionAsync(string sessionId, string userId)
    {
        var existingSession = await GetOwnedSessionAsync(sessionId, userId);
        Deck? deck = null;
        List<string> cardIds;
        List<string> selectedFolderIds;

        if (!string.IsNullOrWhiteSpace(existingSession.DeckId))
        {
            deck = await GetReadableDeckAsync(existingSession.DeckId, userId);
            cardIds = ResolveCardScope(deck, existingSession.CardIds);
            selectedFolderIds = ResolveSelectedFolderIds(deck, cardIds);
        }
        else
        {
            cardIds = NormalizeRequestedCardIds(existingSession.CardIds);
            var cards = await _unitOfWork.Cards.GetStudyCardsByIdsAsync(cardIds);
            if (cards.Count != cardIds.Count)
                throw new AppException(MessageConstants.LearningMessage.INVALID_SCOPE, 400);

            selectedFolderIds = new List<string>();
        }

        if (cardIds.Count == 0)
            throw new AppException(MessageConstants.LearningMessage.NO_CARDS_AVAILABLE, 400);

        var session = new StudySession
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            DeckId = existingSession.DeckId,
            Mode = existingSession.Mode,
            FlashcardFront = existingSession.FlashcardFront,
            FlashcardBack = existingSession.FlashcardBack,
            MultipleChoiceQuestion = existingSession.MultipleChoiceQuestion,
            ShuffleOptions = existingSession.ShuffleOptions,
            SelectedFolderIds = selectedFolderIds,
            CardIds = cardIds,
        };

        await _unitOfWork.StudySessions.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return session.ToResponse(deck);
    }

    public async Task<TodayReviewSummaryResponse> GetTodayReviewAsync(TodayReviewQuery query, string userId)
    {
        if (string.IsNullOrWhiteSpace(query.DeckId))
        {
            var dueAll = await _unitOfWork.UserCardProgresses.GetDueByUserAsync(userId, DateTime.UtcNow);
            return new TodayReviewSummaryResponse
            {
                DueCount = dueAll.Count,
                TotalCards = dueAll.Count,
            };
        }

        var deck = await GetReadableDeckAsync(query.DeckId, userId);
        var selectedFolderIds = ResolveFolderScope(deck, query.FolderIds);
        var cardIds = deck.Folders
            .Where(folder => selectedFolderIds.Contains(folder.Id))
            .SelectMany(folder => folder.FolderCards)
            .Select(folderCard => folderCard.CardId)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var due = cardIds.Count == 0
            ? new List<UserCardProgress>()
            : await _unitOfWork.UserCardProgresses.GetDueByUserAndCardIdsAsync(userId, cardIds, DateTime.UtcNow);

        return new TodayReviewSummaryResponse
        {
            DeckId = deck.Id,
            FolderIds = selectedFolderIds,
            DueCount = due.Count,
            TotalCards = cardIds.Count,
        };
    }

    public async Task<DueReviewCardsResponse> GetDueCardsAsync(DueReviewCardsQuery query, string userId)
    {
        var dueProgresses = await _unitOfWork.UserCardProgresses.GetDueByUserAsync(userId, DateTime.UtcNow);
        var orderedCardIds = dueProgresses
            .OrderBy(x => x.NextReviewAt)
            .Select(x => x.CardId)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return new DueReviewCardsResponse
        {
            DueCount = orderedCardIds.Count,
            CardIds = orderedCardIds,
        };
    }

    private async Task<StudyQuestionResponse> BuildQuestionResponseAsync(StudySession session, Card card, UserCardProgress? progress)
    {
        var payload = BuildAnswerPayload(card, progress, session);
        var response = new StudyQuestionResponse
        {
            SessionId = session.Id,
            CardId = card.Id,
            CardType = card.CardType.ToString(),
            Mode = session.Mode.ToString(),
            Prompt = payload.Prompt,
            QuestionText = payload.QuestionText,
            SecondaryText = payload.SecondaryText,
            Hint = payload.Hint,
            FrontText = payload.FrontText,
            BackText = payload.BackText,
            AllowsMultipleSelection = payload.AcceptedAnswers.Count > 1,
            IsCompleted = false,
        };

        if (session.Mode == StudyMode.MultipleChoice)
        {
            response.Options = await BuildOptionsAsync(session, card, payload.AcceptedAnswers);
            response.AllowsMultipleSelection = false;
        }

        return response;
    }

    private AnswerPayload BuildAnswerPayload(Card card, UserCardProgress? progress, StudySession session)
    {
        return session.Mode switch
        {
            StudyMode.FillInBlank => BuildFillInBlankPayload(card, progress),
            StudyMode.MultipleChoice => BuildMultipleChoicePayload(card, session),
            StudyMode.Flashcard => BuildFlashcardPayload(card, session),
            _ => throw new AppException(MessageConstants.LearningMessage.INVALID_MODE, 400),
        };
    }

    private AnswerPayload BuildFillInBlankPayload(Card card, UserCardProgress? progress)
    {
        var selected = SelectCardSentence(card, progress);
        if (selected != null)
        {
            var acceptedAnswers = StringHelper.NormalizeAnswerList(
                selected.AnswerList,
                selected.BlankWord);

            if (acceptedAnswers.Count == 0)
                acceptedAnswers = LearningHelper.BuildFallbackAnswers(card);

            var blankValue = selected.BlankWord ?? acceptedAnswers.FirstOrDefault() ?? string.Empty;
            return new AnswerPayload(
                "Điền vào chỗ trống",
                LearningHelper.ReplaceFirstBlank(selected.Sentence.Text, blankValue),
                selected.Sentence.Meaning,
                selected.Hint,
                acceptedAnswers,
                selected.SentenceId,
                null,
                null);
        }

        var fallbackAnswers = LearningHelper.BuildFallbackAnswers(card);
        return new AnswerPayload(
            "Điền đáp án phù hợp cho thẻ",
            card.Summary,
            card.Title,
            null,
            fallbackAnswers,
            null,
            null,
            null);
    }

    private AnswerPayload BuildMultipleChoicePayload(Card card, StudySession session)
    {
        var question = session.MultipleChoiceQuestion == MultipleChoiceQuestionType.SummaryToTitle
            ? card.Summary
            : card.Title;
        var answer = session.MultipleChoiceQuestion == MultipleChoiceQuestionType.SummaryToTitle
            ? card.Title
            : card.Summary;

        return new AnswerPayload(
            session.MultipleChoiceQuestion == MultipleChoiceQuestionType.SummaryToTitle
                ? "Chọn từ khóa đúng của nghĩa"
                : "Chọn nghĩa đúng của thẻ",
            question,
            null,
            null,
            new List<string> { answer },
            null,
            null,
            null);
    }

    private AnswerPayload BuildFlashcardPayload(Card card, StudySession session)
    {
        return new AnswerPayload(
            "Xem flashcard rồi đánh dấu đang học hoặc đã biết",
            null,
            null,
            null,
            new List<string>(),
            null,
            ResolveFlashcardContent(card, session.FlashcardFront),
            ResolveFlashcardContent(card, session.FlashcardBack));
    }

    private async Task<List<StudyQuestionOptionResponse>> BuildOptionsAsync(StudySession session, Card card, List<string> acceptedAnswers)
    {
        var options = new List<string>(acceptedAnswers);
        var distractors = new List<string>();

        foreach (var candidateId in session.CardIds.Where(id => id != card.Id))
        {
            var candidateCard = await _unitOfWork.Cards.GetStudyCardByIdAsync(candidateId);
            if (candidateCard == null || candidateCard.CardType != card.CardType)
                continue;

            distractors.Add(session.MultipleChoiceQuestion == MultipleChoiceQuestionType.SummaryToTitle
                ? candidateCard.Title
                : candidateCard.Summary);
            if (options.Count + distractors.Distinct(StringComparer.OrdinalIgnoreCase).Count() >= 4)
                break;
        }

        if (options.Count + distractors.Distinct(StringComparer.OrdinalIgnoreCase).Count() < 4)
        {
            var (globalCards, _) = await _unitOfWork.Cards.SearchCardsAsync(card.CardType, null, null, 1, 50);
            foreach (var globalCard in globalCards.Where(x => x.Id != card.Id && !session.CardIds.Contains(x.Id, StringComparer.Ordinal)))
            {
                var candidateCard = await _unitOfWork.Cards.GetStudyCardByIdAsync(globalCard.Id);
                if (candidateCard == null || candidateCard.CardType != card.CardType)
                    continue;

                distractors.Add(session.MultipleChoiceQuestion == MultipleChoiceQuestionType.SummaryToTitle
                    ? candidateCard.Title
                    : candidateCard.Summary);
                if (options.Count + distractors.Distinct(StringComparer.OrdinalIgnoreCase).Count() >= 4)
                    break;
            }
        }

        options.AddRange(
            LearningHelper.DistinctByRandomOrder(distractors, _random)
                .Where(option => !acceptedAnswers.Contains(option, StringComparer.OrdinalIgnoreCase))
                .Take(Math.Max(4 - options.Count, 0)));

        var finalOptions = session.ShuffleOptions
            ? LearningHelper.DistinctByRandomOrder(options, _random)
            : options
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

        return finalOptions
            .Select(option => new StudyQuestionOptionResponse
            {
                Id = option,
                Text = option,
            })
            .ToList();
    }

    private static bool EvaluateSubmission(AnswerPayload payload, SubmitStudyAnswerRequest request, StudyMode mode)
    {
        return mode switch
        {
            StudyMode.FillInBlank => request.Answers
                .Select(answer => answer.Trim())
                .Any(answer => payload.AcceptedAnswers.Contains(answer, StringComparer.OrdinalIgnoreCase)),
            StudyMode.MultipleChoice => payload.AcceptedAnswers.Count > 0
                && payload.AcceptedAnswers.Count == request.SelectedOptionIds.Count
                && payload.AcceptedAnswers.All(answer => request.SelectedOptionIds.Contains(answer, StringComparer.OrdinalIgnoreCase)),
            StudyMode.Flashcard => ParseFlashcardResult(request.FlashcardResult) == FlashcardReviewResult.Known,
            _ => false,
        };
    }

    private static void ApplyProgress(UserCardProgress progress, StudyMode mode, SubmitStudyAnswerRequest request, bool isCorrect, DateTime now)
    {
        if (mode == StudyMode.Flashcard)
        {
            var result = ParseFlashcardResult(request.FlashcardResult);
            if (result == FlashcardReviewResult.Learning)
            {
                progress.SrsLevel = SrsLevel.level_1;
                progress.ConsecutiveCorrect = 0;
            }
            else
            {
                progress.SrsLevel = LearningHelper.IncreaseLevel(progress.SrsLevel);
                progress.ConsecutiveCorrect++;
            }
        }
        else if (isCorrect)
        {
            progress.SrsLevel = LearningHelper.IncreaseLevel(progress.SrsLevel);
            progress.ConsecutiveCorrect++;
        }
        else
        {
            progress.SrsLevel = SrsLevel.level_1;
            progress.ConsecutiveCorrect = 0;
        }

        progress.IsMastered = progress.SrsLevel == SrsLevel.level_12;
        progress.NextReviewAt = LearningHelper.ResolveNextReviewAt(progress.SrsLevel, now);
    }

    private async Task<UserCardProgress> CreateProgressAsync(string userId, string cardId)
    {
        var progress = new UserCardProgress
        {
            UserId = userId,
            CardId = cardId,
            NextReviewAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _unitOfWork.UserCardProgresses.AddAsync(progress);
        return progress;
    }

    private async Task<StudySession> GetOwnedSessionAsync(string sessionId, string userId)
    {
        return await _unitOfWork.StudySessions.GetByIdForUserAsync(sessionId, userId)
            ?? throw new AppException(MessageConstants.LearningMessage.SESSION_NOT_FOUND, 404);
    }

    private async Task<Deck?> GetSessionDeckAsync(StudySession session, string userId)
    {
        if (string.IsNullOrWhiteSpace(session.DeckId))
            return null;

        return await GetReadableDeckAsync(session.DeckId, userId);
    }

    private async Task<Deck> GetReadableDeckAsync(string deckId, string userId)
    {
        var deck = await _unitOfWork.Decks.GetDetailByIdAsync(deckId, userId)
            ?? throw new AppException(MessageConstants.DeckMessage.NOT_FOUND, 404);

        var isReadable = (deck.Status == PublishStatus.Published && deck.Visibility == DeckVisibility.Public) || deck.CreatedBy == userId;
        if (!isReadable)
            throw new AppException(MessageConstants.DeckMessage.READ_FORBIDDEN, 403);

        return deck;
    }

    private List<string> ResolveFolderScope(Deck deck, List<string>? requestedFolderIds)
    {
        var allFolderIds = deck.Folders.Select(folder => folder.Id).ToList();
        if (requestedFolderIds == null || requestedFolderIds.Count == 0)
            return allFolderIds;

        var normalizedFolderIds = requestedFolderIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (normalizedFolderIds.Count == 0)
            return allFolderIds;

        if (normalizedFolderIds.Any(folderId => !allFolderIds.Contains(folderId, StringComparer.Ordinal)))
            throw new AppException(MessageConstants.LearningMessage.INVALID_SCOPE, 400);

        return normalizedFolderIds;
    }

    private static List<string> ResolveCardScope(Deck deck, List<string>? requestedCardIds)
    {
        var allCardIds = deck.Folders
            .SelectMany(folder => folder.FolderCards)
            .OrderBy(folderCard => folderCard.Position)
            .Select(folderCard => folderCard.CardId)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (requestedCardIds == null || requestedCardIds.Count == 0)
            return allCardIds;

        var normalizedCardIds = requestedCardIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (normalizedCardIds.Count == 0)
            return allCardIds;

        if (normalizedCardIds.Any(cardId => !allCardIds.Contains(cardId, StringComparer.Ordinal)))
            throw new AppException(MessageConstants.LearningMessage.INVALID_SCOPE, 400);

        return normalizedCardIds;
    }

    private static List<string> ResolveSelectedFolderIds(Deck deck, List<string> cardIds)
    {
        return deck.Folders
            .Where(folder => folder.FolderCards.Any(folderCard => cardIds.Contains(folderCard.CardId, StringComparer.Ordinal)))
            .Select(folder => folder.Id)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static List<string> NormalizeRequestedCardIds(IEnumerable<string> cardIds)
    {
        return cardIds
            .Where(cardId => !string.IsNullOrWhiteSpace(cardId))
            .Select(cardId => cardId.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private async Task<Card> GetStudyCardRequiredAsync(string cardId)
    {
        return await _unitOfWork.Cards.GetStudyCardByIdAsync(cardId)
            ?? throw new AppException(MessageConstants.DeckMessage.CARD_NOT_FOUND, 404);
    }

    private async Task<ResolvedStudySessionSettings> ResolveSessionSettingsAsync(string userId, StudySessionSettingsRequest? request)
    {
        var userDefaults = await _unitOfWork.UserLearningSettings.GetByUserIdAsync(userId);

        var flashcardFront = ResolveEnumSetting(
            request?.FlashcardFront,
            userDefaults?.FlashcardFront,
            FlashcardContentType.Title);

        var flashcardBack = ResolveEnumSetting(
            request?.FlashcardBack,
            userDefaults?.FlashcardBack,
            FlashcardContentType.Summary);

        var multipleChoiceQuestion = ResolveEnumSetting(
            request?.MultipleChoiceQuestion,
            userDefaults?.MultipleChoiceQuestion,
            MultipleChoiceQuestionType.TitleToSummary);

        var shuffleOptions = request?.ShuffleOptions
            ?? userDefaults?.ShuffleOptions
            ?? true;

        return new ResolvedStudySessionSettings(
            flashcardFront,
            flashcardBack,
            multipleChoiceQuestion,
            shuffleOptions);
    }

    private static string ResolveFlashcardContent(Card card, FlashcardContentType contentType)
    {
        return contentType switch
        {
            FlashcardContentType.Summary => card.Summary,
            _ => card.Title,
        };
    }

    private static TEnum ResolveEnumSetting<TEnum>(string? requestValue, TEnum? userValue, TEnum fallback)
        where TEnum : struct, Enum
    {
        if (!string.IsNullOrWhiteSpace(requestValue))
            return EnumParsingHelper.ParseRequired<TEnum>(requestValue);

        return userValue ?? fallback;
    }

    private static int NormalizeLimit(int? value, int defaultValue, int maxValue)
    {
        if (!value.HasValue || value.Value <= 0)
            return defaultValue;

        return Math.Min(value.Value, maxValue);
    }

    private static CardProgressResponse MapCardProgress(Card card, UserCardProgress progress)
    {
        return new CardProgressResponse
        {
            CardId = card.Id,
            CardType = card.CardType.ToString(),
            Title = card.Title,
            Summary = card.Summary,
            SrsLevel = progress.SrsLevel.ToString(),
            NextReviewAt = progress.NextReviewAt,
            LastReviewedAt = progress.LastReviewedAt,
            ConsecutiveCorrect = progress.ConsecutiveCorrect,
            IsMastered = progress.IsMastered,
            LastSentenceId = progress.LastSentenceId,
        };
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

    private static FlashcardReviewResult ParseFlashcardResult(string? result)
    {
        try
        {
            return EnumParsingHelper.ParseRequired<FlashcardReviewResult>(result ?? string.Empty);
        }
        catch (ApplicationException)
        {
            throw new AppException(MessageConstants.LearningMessage.INVALID_SUBMISSION, 400);
        }
    }

    private static CardSentence? SelectCardSentence(Card card, UserCardProgress? progress)
    {
        var sentences = card.CardSentences
            .Where(cs => cs.Sentence != null)
            .OrderBy(cs => cs.Position)
            .ToList();

        if (sentences.Count == 0)
            return null;

        if (!string.IsNullOrWhiteSpace(progress?.LastSentenceId))
        {
            var nextSentence = sentences.FirstOrDefault(cs => cs.SentenceId != progress.LastSentenceId);
            if (nextSentence != null)
                return nextSentence;
        }

        return sentences.First();
    }

    private sealed record AnswerPayload(
        string Prompt,
        string? QuestionText,
        string? SecondaryText,
        string? Hint,
        List<string> AcceptedAnswers,
        string? SelectedSentenceId,
        string? FrontText,
        string? BackText);

    private sealed record ResolvedStudySessionSettings(
        FlashcardContentType FlashcardFront,
        FlashcardContentType FlashcardBack,
        MultipleChoiceQuestionType MultipleChoiceQuestion,
        bool ShuffleOptions);
}
