namespace Application.IRepositories;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IMediaAssetRepository MediaAssets { get; }
    
    ICardRepository Cards { get; }
    IVocabularyDetailRepository VocabularyDetails { get; }
    IGrammarDetailRepository GrammarDetails { get; }
    IKanjiDetailRepository KanjiDetails { get; }
    IRadicalDetailRepository RadicalDetails { get; }
    IKanjiRadicalRepository KanjiRadicals { get; }
    IGrammarRelationRepository GrammarRelations { get; }
    IGrammarResourceRepository GrammarResources { get; }
    ISentenceRepository Sentences { get; }
    ICardSentenceRepository CardSentences { get; }
    IUserCardNoteRepository UserCardNotes { get; }
    INoteLikeRepository NoteLikes { get; }

    Task<int> SaveChangesAsync();
}
