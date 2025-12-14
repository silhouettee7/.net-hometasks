using RESTAuth.Domain.Dtos;
using RESTAuth.Domain.Models;

namespace RESTAuth.Domain.Abstractions.Services;

public interface IUserService
{
    Task<AppResult<Guid>> RegisterUser(UserDtoRequest dto);
    Task<AppResult> EditUser(Guid id, UserDtoRequest dto);
    Task<AppResult> DeleteUser(Guid id);
    Task<AppResult<CursorPaginationResponse<UserDtoResponse>>> GetUsersPageForPeriodByRegistrationDate(
        CursorPaginationRequest request,DateTime startDate, DateTime endDate);
    Task<AppResult<CursorPaginationResponse<UserDtoResponse>>> GetUsersPageForPeriodByUpdatingDate(
        CursorPaginationRequest request, DateTime startDate, DateTime endDate);
    Task<AppResult<List<DepartmentAverageSalaryDto>>> GetUserDepartmentAverageSalaries();
    Task<AppResult<string>> GenerateReportOnUsersAndReturnLink(Guid reportId);
}