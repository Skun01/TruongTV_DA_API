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
        var deck = await GetReadableDeckAsync(request.DeckId, userId);
        var selectedFolderIds = ResolveFolderScope(deck, request.FolderIds);
        var cardIds = deck.Folders
            .Where(folder => selectedFolderIds.Contains(folder.Id))
            .SelectMany(folder => folder.FolderCards)
            .OrderBy(folderCard => folderCard.Position)
            .Select(folderCard => folderCard.CardId)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (cardIds.Count == 0)
            throw new AppException(MessageConstants.LearningMessage.NO_CARDS_AVAILABLE, 400);

        var session = new StudySession
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            DeckId = deck.Id,
            Mode = mode,
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
        var deck = await GetReadableDeckAsync(session.DeckId, userId);
        return session.ToResponse(deck);
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

        var payload = BuildAnswerPayload(card, progress, session.Mode);
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

    private async Task<StudyQuestionResponse> BuildQuestionResponseAsync(StudySession session, Card card, UserCardProgress? progress)
    {
        var payload = BuildAnswerPayload(card, progress, session.Mode);
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
            response.AllowsMultipleSelection = payload.AcceptedAnswers.Count > 1;
        }

        return response;
    }

    private AnswerPayload BuildAnswerPayload(Card card, UserCardProgress? progress, StudyMode mode)
    {
        return mode switch
        {
            StudyMode.FillInBlank => BuildFillInBlankPayload(card, progress),
            StudyMode.MultipleChoice => BuildMultipleChoicePayload(card, progress),
            StudyMode.Flashcard => BuildFlashcardPayload(card),
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

    private AnswerPayload BuildMultipleChoicePayload(Card card, UserCardProgress? progress)
    {
        return new AnswerPayload(
            "Chọn nghĩa đúng của thẻ",
            card.Title,
            null,
            null,
            new List<string> { card.Summary },
            null,
            null,
            null);
    }

    private AnswerPayload BuildFlashcardPayload(Card card)
    {
        return new AnswerPayload(
            "Xem flashcard rồi đánh dấu đang học hoặc đã biết",
            null,
            null,
            null,
            new List<string>(),
            null,
            card.Title,
            card.Summary);
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

            distractors.Add(candidateCard.Summary);
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

                distractors.Add(candidateCard.Summary);
                if (options.Count + distractors.Distinct(StringComparer.OrdinalIgnoreCase).Count() >= 4)
                    break;
            }
        }

        options.AddRange(
            LearningHelper.DistinctByRandomOrder(distractors, _random)
                .Where(option => !acceptedAnswers.Contains(option, StringComparer.OrdinalIgnoreCase))
                .Take(Math.Max(4 - options.Count, 0)));

        return LearningHelper.DistinctByRandomOrder(options, _random)
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
                progress.SrsLevel = SrsLevel.Level1;
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
            progress.SrsLevel = SrsLevel.Level1;
            progress.ConsecutiveCorrect = 0;
        }

        progress.IsMastered = progress.SrsLevel == SrsLevel.Level5;
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

    private async Task<Card> GetStudyCardRequiredAsync(string cardId)
    {
        return await _unitOfWork.Cards.GetStudyCardByIdAsync(cardId)
            ?? throw new AppException(MessageConstants.DeckMessage.CARD_NOT_FOUND, 404);
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
}
