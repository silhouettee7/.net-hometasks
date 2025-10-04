using RESTAuth.Api.Enums;
using RESTAuth.Domain.Dtos;

namespace RESTAuth.Api.Models;

public class UsersPageWithPeriodDateDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public UserDateOption Option { get; set; }
    public required CursorPaginationRequest CursorPaginationRequest { get; set; }
}