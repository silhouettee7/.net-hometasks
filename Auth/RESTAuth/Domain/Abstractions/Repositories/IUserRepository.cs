using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Domain.Abstractions.Repositories;

public interface IUserRepository: IRepository<User, Guid>
{
    Task<Result<Dictionary<string,decimal>>> GetUserAverageSalariesByDepartment();
    Task<Result<User>> GetUserByEmail(string email);
    Task CreateUsers(IEnumerable<User> users);
}