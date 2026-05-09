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
    private const int MaxAttemptsPerCard = 2;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Random _random = new();

    public LearningService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StudySessionResponse> CreateSessionAsync(CreateStudySessionRequest request, string userId)
    {
        var mode = LearningHelper.ParseStudyMode(request.Mode);
        var settings = await ResolveSessionSettingsAsync(userId, request.Settings);
        Deck? deck = null;
        List<string> cardIds;
        List<string> selectedFolderIds;

        if (!string.IsNullOrWhiteSpace(request.DeckId))
        {
            deck = await GetReadableDeckAsync(request.DeckId, userId);
            cardIds = LearningSessionHelper.ResolveCardScope(deck, request.CardIds);
            selectedFolderIds = LearningSessionHelper.ResolveSelectedFolderIds(deck, cardIds);
        }
        else
        {
            cardIds = LearningHelper.NormalizeRequestedIds(request.CardIds);
            if (cardIds.Count == 0)
                throw new AppException(MessageConstants.LearningMessage.NO_CARDS_AVAILABLE, 400);

            var cards = await _unitOfWork.Cards.GetStudyCardsByIdsAsync(cardIds);
            if (cards.Count != cardIds.Count)
                throw new AppException(MessageConstants.LearningMessage.INVALID_SCOPE, 400);

            selectedFolderIds = new List<string>();
        }

        if (cardIds.Count == 0)
            throw new AppException(MessageConstants.LearningMessage.NO_CARDS_AVAILABLE, 400);

        var (eligibleCardIds, skippedCardIds) = await ResolveEligibleCardScopeAsync(cardIds, mode, deck == null);
        if (eligibleCardIds.Count == 0)
            throw new AppException(MessageConstants.LearningMessage.NO_CARDS_AVAILABLE, 400);

        selectedFolderIds = deck == null
            ? new List<string>()
            : LearningSessionHelper.ResolveSelectedFolderIds(deck, eligibleCardIds);

        var session = LearningSessionHelper.CreateSession(
            userId,
            deck?.Id,
            mode,
            settings,
            selectedFolderIds,
            eligibleCardIds,
            skippedCardIds);

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
        var limit = LearningHelper.NormalizeLimit(query.Limit, 20, 100);
        var sessions = await _unitOfWork.StudySessions.GetRecentByUserAsync(userId, limit);
        var deckIds = sessions
            .Where(x => !string.IsNullOrWhiteSpace(x.DeckId))
            .Select(x => x.DeckId!)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var deckMap = new Dictionary<string, Deck>(StringComparer.Ordinal);
        foreach (var deckId in deckIds)
        {
            var deck = await GetReadableDeckAsync(deckId, userId);
            deckMap[deck.Id] = deck;
        }

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

        var nextCardId = session.CardIds.FirstOrDefault(cardId =>
            !session.CompletedCardIds.Contains(cardId, StringComparer.Ordinal)
            && !session.RetryCardIds.Contains(cardId, StringComparer.Ordinal))
            ?? session.RetryCardIds.FirstOrDefault(cardId => !session.CompletedCardIds.Contains(cardId, StringComparer.Ordinal));
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

        ValidateSubmissionForMode(session.Mode, request);

        var card = await GetStudyCardRequiredAsync(request.CardId);
        var progress = await _unitOfWork.UserCardProgresses.GetByUserAndCardIdAsync(userId, request.CardId);
        var isNewProgress = progress == null;
        progress ??= await CreateProgressAsync(userId, request.CardId);

        var payload = LearningQuestionHelper.BuildAnswerPayload(card, progress, session);
        var now = DateTime.UtcNow;
        var attemptNo = GetAttemptNo(session, request.CardId);
        var matchResult = session.Mode == StudyMode.FillInBlank
            ? LearningAnswerMatcher.Match(request.Answers, payload.AcceptedAnswers)
            : new LearningAnswerMatchResult(
                LearningQuestionHelper.EvaluateSubmission(payload, request, session.Mode),
                request.Answers.Where(answer => !string.IsNullOrWhiteSpace(answer)).Select(answer => answer.Trim()).ToList(),
                new List<string>(),
                payload.AcceptedAnswers.FirstOrDefault());
        var isCorrect = matchResult.IsCorrect;
        var isFinalAttempt = isCorrect || attemptNo >= MaxAttemptsPerCard;
        var willRepeat = !isCorrect && !isFinalAttempt;

        LearningQuestionHelper.ApplyProgress(progress, session.Mode, request, isCorrect, now);
        if (!string.IsNullOrWhiteSpace(payload.SelectedSentenceId))
            progress.LastSentenceId = payload.SelectedSentenceId;

        progress.LastReviewedAt = now;
        progress.UpdatedAt = now;

        if (isCorrect)
            session.CorrectCount++;
        else
            session.IncorrectCount++;

        if (willRepeat)
        {
            if (!session.RetryCardIds.Contains(request.CardId, StringComparer.Ordinal))
                session.RetryCardIds.Add(request.CardId);
        }
        else
        {
            session.RetryCardIds.RemoveAll(cardId => string.Equals(cardId, request.CardId, StringComparison.Ordinal));
            if (!session.CompletedCardIds.Contains(request.CardId, StringComparer.Ordinal))
                session.CompletedCardIds.Add(request.CardId);
        }

        if (session.CompletedCardIds.Count >= session.CardIds.Count && session.RetryCardIds.Count == 0)
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
            CanonicalAnswer = matchResult.CanonicalAnswer,
            SubmittedAnswers = matchResult.SubmittedAnswers,
            NormalizedSubmittedAnswers = matchResult.NormalizedSubmittedAnswers,
            CompletedQuestionText = payload.CompletedQuestionText,
            SentenceId = payload.SelectedSentenceId,
            AttemptNo = attemptNo,
            MaxAttempts = MaxAttemptsPerCard,
            WillRepeat = willRepeat,
            IsFinalAttempt = isFinalAttempt,
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
            return card.ToDefaultCardProgressResponse(DateTime.UtcNow);

        return card.ToCardProgressResponse(progress);
    }

    public async Task<StudySessionResultResponse> GetSessionResultAsync(string sessionId, string userId)
    {
        var session = await GetOwnedSessionAsync(sessionId, userId);
        var deck = await GetSessionDeckAsync(session, userId);
        return session.ToResultResponse(deck);
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
            cardIds = LearningSessionHelper.ResolveCardScope(deck, existingSession.CardIds);
            selectedFolderIds = LearningSessionHelper.ResolveSelectedFolderIds(deck, cardIds);
        }
        else
        {
            cardIds = LearningHelper.NormalizeRequestedIds(existingSession.CardIds);
            var cards = await _unitOfWork.Cards.GetStudyCardsByIdsAsync(cardIds);
            if (cards.Count != cardIds.Count)
                throw new AppException(MessageConstants.LearningMessage.INVALID_SCOPE, 400);

            selectedFolderIds = new List<string>();
        }

        if (cardIds.Count == 0)
            throw new AppException(MessageConstants.LearningMessage.NO_CARDS_AVAILABLE, 400);

        var (eligibleCardIds, skippedCardIds) = await ResolveEligibleCardScopeAsync(cardIds, existingSession.Mode, string.IsNullOrWhiteSpace(existingSession.DeckId));
        if (eligibleCardIds.Count == 0)
            throw new AppException(MessageConstants.LearningMessage.NO_CARDS_AVAILABLE, 400);

        selectedFolderIds = deck == null
            ? new List<string>()
            : LearningSessionHelper.ResolveSelectedFolderIds(deck, eligibleCardIds);

        var session = LearningSessionHelper.CreateSession(
            userId,
            existingSession.DeckId,
            existingSession.Mode,
            new LearningSessionSettings(
                existingSession.FlashcardFront,
                existingSession.FlashcardBack,
                existingSession.MultipleChoiceQuestion,
                existingSession.ShuffleOptions),
            selectedFolderIds,
            eligibleCardIds,
            skippedCardIds);

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
        var selectedFolderIds = LearningSessionHelper.ResolveFolderScope(deck, query.FolderIds);
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
        var payload = LearningQuestionHelper.BuildAnswerPayload(card, progress, session);
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
            SentenceId = payload.SelectedSentenceId,
            QuestionSource = payload.QuestionSource,
            AttemptNo = GetAttemptNo(session, card.Id),
            MaxAttempts = MaxAttemptsPerCard,
            AcceptedAnswerCount = payload.AcceptedAnswers.Count,
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

    private async Task<List<StudyQuestionOptionResponse>> BuildOptionsAsync(StudySession session, Card card, List<string> acceptedAnswers)
    {
        var distractors = new List<string>();
        var sessionCandidateIds = session.CardIds
            .Where(id => id != card.Id)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (sessionCandidateIds.Count > 0)
        {
            var sessionCandidateCards = await _unitOfWork.Cards.GetStudyCardsByIdsAsync(sessionCandidateIds);
            distractors.AddRange(sessionCandidateCards
                .Where(candidateCard => candidateCard.CardType == card.CardType)
                .Select(candidateCard => LearningQuestionHelper.ResolveMultipleChoiceValue(candidateCard, session.MultipleChoiceQuestion)));
        }

        if (acceptedAnswers.Count + distractors.Distinct(StringComparer.OrdinalIgnoreCase).Count() < 4)
        {
            var (globalCards, _) = await _unitOfWork.Cards.SearchCardsAsync(card.CardType, null, null, 1, 50);
            distractors.AddRange(globalCards
                .Where(globalCard => globalCard.Id != card.Id && !session.CardIds.Contains(globalCard.Id, StringComparer.Ordinal))
                .Select(globalCard => LearningQuestionHelper.ResolveMultipleChoiceValue(globalCard, session.MultipleChoiceQuestion)));
        }

        return LearningQuestionHelper.BuildMultipleChoiceOptions(
            acceptedAnswers,
            distractors,
            session.ShuffleOptions,
            _random);
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

    private async Task<(List<string> EligibleCardIds, List<string> SkippedCardIds)> ResolveEligibleCardScopeAsync(
        List<string> cardIds,
        StudyMode mode,
        bool requireAllCards)
    {
        var cards = await _unitOfWork.Cards.GetStudyCardsByIdsAsync(cardIds);
        if (requireAllCards && cards.Count != cardIds.Count)
            throw new AppException(MessageConstants.LearningMessage.INVALID_SCOPE, 400);

        var cardMap = cards.ToDictionary(card => card.Id, StringComparer.Ordinal);
        var eligibleCardIds = new List<string>();
        var skippedCardIds = new List<string>();

        foreach (var cardId in cardIds)
        {
            if (cardMap.TryGetValue(cardId, out var card)
                && LearningModeEligibilityHelper.IsCardEligible(card, mode))
            {
                eligibleCardIds.Add(cardId);
                continue;
            }

            skippedCardIds.Add(cardId);
        }

        return (eligibleCardIds, skippedCardIds);
    }

    private static int GetAttemptNo(StudySession session, string cardId)
    {
        return session.RetryCardIds.Contains(cardId, StringComparer.Ordinal)
            ? MaxAttemptsPerCard
            : 1;
    }

    private static void ValidateSubmissionForMode(StudyMode mode, SubmitStudyAnswerRequest request)
    {
        switch (mode)
        {
            case StudyMode.FillInBlank:
                if (!request.Answers.Any(answer => !string.IsNullOrWhiteSpace(answer)))
                    throw new AppException(MessageConstants.LearningMessage.INVALID_SUBMISSION, 400);
                break;
            case StudyMode.MultipleChoice:
                if (request.SelectedOptionIds.Count != 1 || request.SelectedOptionIds.Any(string.IsNullOrWhiteSpace))
                    throw new AppException(MessageConstants.LearningMessage.INVALID_SUBMISSION, 400);
                break;
            case StudyMode.Flashcard:
                LearningHelper.ParseFlashcardResult(request.FlashcardResult);
                break;
            default:
                throw new AppException(MessageConstants.LearningMessage.INVALID_MODE, 400);
        }
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

    private async Task<Card> GetStudyCardRequiredAsync(string cardId)
    {
        return await _unitOfWork.Cards.GetStudyCardByIdAsync(cardId)
            ?? throw new AppException(MessageConstants.DeckMessage.CARD_NOT_FOUND, 404);
    }

    private async Task<LearningSessionSettings> ResolveSessionSettingsAsync(string userId, StudySessionSettingsRequest? request)
    {
        var userDefaults = await _unitOfWork.UserLearningSettings.GetByUserIdAsync(userId);

        var flashcardFront = LearningHelper.ResolveEnumSetting(
            request?.FlashcardFront,
            userDefaults?.FlashcardFront,
            FlashcardContentType.Title);

        var flashcardBack = LearningHelper.ResolveEnumSetting(
            request?.FlashcardBack,
            userDefaults?.FlashcardBack,
            FlashcardContentType.Summary);

        var multipleChoiceQuestion = LearningHelper.ResolveEnumSetting(
            request?.MultipleChoiceQuestion,
            userDefaults?.MultipleChoiceQuestion,
            MultipleChoiceQuestionType.TitleToSummary);

        var shuffleOptions = request?.ShuffleOptions
            ?? userDefaults?.ShuffleOptions
            ?? true;

        return new LearningSessionSettings(
            flashcardFront,
            flashcardBack,
            multipleChoiceQuestion,
            shuffleOptions);
    }
}
