using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Domain.Abstractions.Repositories;

public interface IUserRepository: IRepository<User, Guid>
{
    Task<AppResult<Dictionary<string,decimal>>> GetUserAverageSalariesByDepartment();
    Task<AppResult<User>> GetUserByEmail(string email);
    Task CreateUsers(IEnumerable<User> users);
    Task<List<UserReport>> GetAllUsers();
}