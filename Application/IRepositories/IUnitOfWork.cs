namespace Application.IRepositories;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IDeckRepository Decks { get; }
    IVocabularyCardRepository VocabularyCards { get; }
    IExampleSentenceRepository ExampleSentences { get; }
    IGrammarCardRepository GrammarCards { get; }
    IUserSettingsRepository UserSettings { get; }
    IDeckQueueRepository DeckQueues { get; }
    ICardProgressRepository CardProgresses { get; }
    IReviewLogRepository ReviewLogs { get; }

    Task<int> SaveChangesAsync();
}

