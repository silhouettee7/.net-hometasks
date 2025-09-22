using RESTAuth.Domain.Dtos;
using RESTAuth.Domain.Models;

namespace RESTAuth.Domain.Abstractions.Services;

public interface IUserService
{
    Task<Result> RegisterUser(UserDtoRequest dto);
    Task<Result> EditUser(Guid id, UserDtoRequest dto);
    Task<Result> DeleteUser(Guid id);
    Task<Result<List<UserDtoResponse>>> GetUsersForPeriodByRegistrationDate(DateTime startDate, DateTime endDate);
    Task<Result<List<UserDtoResponse>>> GetUsersForPeriodByUpdatingDate(DateTime startDate, DateTime endDate);
}