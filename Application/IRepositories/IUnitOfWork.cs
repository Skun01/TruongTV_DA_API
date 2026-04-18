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

    Task<int> SaveChangesAsync();
}
