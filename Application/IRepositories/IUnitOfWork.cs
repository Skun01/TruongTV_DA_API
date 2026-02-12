namespace Application.IRepositories;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IDeckRepository Decks { get; }
    IVocabularyCardRepository VocabularyCards { get; }
    IExampleSentenceRepository ExampleSentences { get; }
    IGrammarCardRepository GrammarCards { get; }

    Task<int> SaveChangesAsync();
}
