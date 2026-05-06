using Domain.Entities;

namespace Application.IRepositories;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IMediaAssetRepository MediaAssets { get; }
    
    ICardRepository Cards { get; }
    IDeckTypeRepository DeckTypes { get; }
    IDeckRepository Decks { get; }
    IRepository<DeckFolder> DeckFolders { get; }
    IRepository<FolderCard> FolderCards { get; }
    IDeckBookmarkRepository DeckBookmarks { get; }
    IVocabularyDetailRepository VocabularyDetails { get; }
    IGrammarDetailRepository GrammarDetails { get; }
    IKanjiDetailRepository KanjiDetails { get; }
    IRadicalDetailRepository RadicalDetails { get; }
    IKanjiRadicalRepository KanjiRadicals { get; }
    IGrammarRelationRepository GrammarRelations { get; }
    IGrammarResourceRepository GrammarResources { get; }
    ISentenceRepository Sentences { get; }
    ICardSentenceRepository CardSentences { get; }
    IUserCardProgressRepository UserCardProgresses { get; }
    IStudySessionRepository StudySessions { get; }
    IUserLearningSettingsRepository UserLearningSettings { get; }
    IUserCardNoteRepository UserCardNotes { get; }
    INoteLikeRepository NoteLikes { get; }
    IShadowingTopicRepository ShadowingTopics { get; }
    IShadowingTopicSentenceRepository ShadowingTopicSentences { get; }
    IShadowingAttemptRepository ShadowingAttempts { get; }

    // JLPT Exam module
    IExamRepository Exams { get; }
    IExamSectionRepository ExamSections { get; }
    IQuestionGroupRepository QuestionGroups { get; }
    IQuestionRepository Questions { get; }
    IRepository<QuestionOption> QuestionOptions { get; }
    IExamSessionRepository ExamSessions { get; }
    IRepository<SessionAnswer> SessionAnswers { get; }
    IRepository<SessionSectionScore> SessionSectionScores { get; }
    IExamSessionAiAnalysisRepository ExamSessionAiAnalyses { get; }
    IAiGeneratedQuestionRepository AiGeneratedQuestions { get; }

    // Conversation module
    IConversationSessionRepository ConversationSessions { get; }
    IConversationMessageRepository ConversationMessages { get; }
    IExtractedVocabularyRepository ExtractedVocabularies { get; }

    Task<int> SaveChangesAsync();
}
