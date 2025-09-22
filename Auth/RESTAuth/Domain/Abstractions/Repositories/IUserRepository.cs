using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Domain.Abstractions.Repositories;

public interface IUserRepository: IRepository<User, Guid>
{
    Task<Result<List<User>>> GetUsersForPeriodByRegistrationDate(DateTime startDate, DateTime endDate);
    Task<Result<List<User>>> GetUsersForPeriodByUpdatedDate(DateTime startDate, DateTime endDate);
    Task<Result<User>> GetUserByEmail(string email);
}