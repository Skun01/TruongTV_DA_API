using Application.IRepositories;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IUserRepository? _users;
    private IRefreshTokenRepository? _refreshTokens;
    private IMediaAssetRepository? _mediaAssets;
    
    private ICardRepository? _cards;
    private IDeckTypeRepository? _deckTypes;
    private IDeckRepository? _decks;
    private IRepository<DeckFolder>? _deckFolders;
    private IRepository<FolderCard>? _folderCards;
    private IDeckBookmarkRepository? _deckBookmarks;
    private IVocabularyDetailRepository? _vocabularyDetails;
    private IGrammarDetailRepository? _grammarDetails;
    private IKanjiDetailRepository? _kanjiDetails;
    private IRadicalDetailRepository? _radicalDetails;
    private IKanjiRadicalRepository? _kanjiRadicals;
    private IGrammarRelationRepository? _grammarRelations;
    private IGrammarResourceRepository? _grammarResources;
    private ISentenceRepository? _sentences;
    private ICardSentenceRepository? _cardSentences;
    private IUserCardNoteRepository? _userCardNotes;
    private INoteLikeRepository? _noteLikes;
    
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context);
    public IMediaAssetRepository MediaAssets => _mediaAssets ??= new MediaAssetRepository(_context);
    
    public ICardRepository Cards => _cards ??= new CardRepository(_context);
    public IDeckTypeRepository DeckTypes => _deckTypes ??= new DeckTypeRepository(_context);
    public IDeckRepository Decks => _decks ??= new DeckRepository(_context);
    public IRepository<DeckFolder> DeckFolders => _deckFolders ??= new Repository<DeckFolder>(_context);
    public IRepository<FolderCard> FolderCards => _folderCards ??= new Repository<FolderCard>(_context);
    public IDeckBookmarkRepository DeckBookmarks => _deckBookmarks ??= new DeckBookmarkRepository(_context);
    public IVocabularyDetailRepository VocabularyDetails => _vocabularyDetails ??= new VocabularyDetailRepository(_context);
    public IGrammarDetailRepository GrammarDetails => _grammarDetails ??= new GrammarDetailRepository(_context);
    public IKanjiDetailRepository KanjiDetails => _kanjiDetails ??= new KanjiDetailRepository(_context);
    public IRadicalDetailRepository RadicalDetails => _radicalDetails ??= new RadicalDetailRepository(_context);
    public IKanjiRadicalRepository KanjiRadicals => _kanjiRadicals ??= new KanjiRadicalRepository(_context);
    public IGrammarRelationRepository GrammarRelations => _grammarRelations ??= new GrammarRelationRepository(_context);
    public IGrammarResourceRepository GrammarResources => _grammarResources ??= new GrammarResourceRepository(_context);
    public ISentenceRepository Sentences => _sentences ??= new SentenceRepository(_context);
    public ICardSentenceRepository CardSentences => _cardSentences ??= new CardSentenceRepository(_context);
    public IUserCardNoteRepository UserCardNotes => _userCardNotes ??= new UserCardNoteRepository(_context);
    public INoteLikeRepository NoteLikes => _noteLikes ??= new NoteLikeRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
