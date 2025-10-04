using System.Collections.Concurrent;
using RESTAuth.Domain.Abstractions.Repositories;
using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Persistence.InMemoryStorage;

public class InMemoryUserRepository(LocalStorage<User, Guid> storage) : InMemoryRepository<User,Guid>(storage), IUserRepository
{
    private ConcurrentDictionary<Guid, User> _users => _data;

    public Task<Result<Dictionary<string, decimal>>> GetUserAverageSalariesByDepartment()
    {
        try
        {
            var result = _users
                .Select(pair => pair.Value)
                .GroupBy(u => u.Department)
                .ToDictionary(group => group.Key,
                    group => group.Average(u => u.Salary));
            return Task.FromResult(
                Result<Dictionary<string, decimal>>.Success(SuccessType.Ok, result));
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                Result<Dictionary<string, decimal>>.Failure(new Error(ErrorType.ServerError, ex.Message)));
        }
    }

    public Task<Result<User>> GetUserByEmail(string email)
    {
        var user = _users.FirstOrDefault(pair => pair.Value.Email == email).Value;
        if (user == null)
        {
            return Task.FromResult(Result<User>.Failure(new Error(ErrorType.NotFound,"User not found")));
        }
        return Task.FromResult(Result<User>.Success(SuccessType.Ok,user));
    }
}