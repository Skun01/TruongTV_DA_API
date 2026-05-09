using Application.DTOs.Common;

namespace Application.DTOs.CardNotes;

public class CardNoteListQuery : PagingQuery
{
    public CardNoteListQuery()
    {
        PageSize = 10;
    }
}
