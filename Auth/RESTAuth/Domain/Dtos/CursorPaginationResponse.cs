namespace RESTAuth.Domain.Dtos;

public class CursorPaginationResponse<T>
{
    public required List<T> Items { get; set; }
    public string? NextCursor { get; set; }
    public string? PreviousCursor { get; set; }
    public required bool HasNext { get; set; }
    public required bool HasPrevious { get; set; }
}