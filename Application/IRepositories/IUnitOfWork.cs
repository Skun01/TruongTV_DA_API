namespace Application.IRepositories;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IDeckRepository Decks { get; }
    IVocabularyCardRepository VocabularyCards { get; }
    IExampleSentenceRepository ExampleSentences { get; }
    IGrammarRepository GrammarCards { get; }

    Task<int> SaveChangesAsync();
}
