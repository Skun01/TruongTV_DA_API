using Application.IRepositories;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IUserRepository? _users;
    private IRefreshTokenRepository? _refreshTokens;
    private IDeckRepository? _decks;
    private IVocabularyCardRepository? _vocabularyCards;
    private IExampleSentenceRepository? _exampleSentences;
    private IGrammarCardRepository? _grammarCards;
    
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context);
    public IDeckRepository Decks => _decks ??= new DeckRepository(_context);

    public IVocabularyCardRepository VocabularyCards => _vocabularyCards ??= new VocabularyCardRepository(_context);
    public IExampleSentenceRepository ExampleSentences => _exampleSentences ??= new ExampleSentenceRepository(_context);
    public IGrammarCardRepository GrammarCards => _grammarCards ??= new GrammarCardRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
