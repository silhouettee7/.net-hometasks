namespace RESTAuth.Domain.Dtos;

public class CursorPaginationRequest
{
    public string? Cursor { get; set; }
    public required int PageSize { get; set; }
    public required bool Forward { get; set; }
}