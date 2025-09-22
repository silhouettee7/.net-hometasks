using System.Collections.Concurrent;
using RESTAuth.Domain.Abstractions.Repositories;
using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Persistence.InMemoryStorage;

public class InMemoryUserRepository: InMemoryRepository<User,Guid>, IUserRepository
{
    private ConcurrentDictionary<Guid, User> _users => _data;
    public Task<Result<List<User>>> GetUsersForPeriodByRegistrationDate(DateTime startDate, DateTime endDate)
    {
        var users = _users
            .Where(pair => pair.Value.CreatedDate >= startDate && pair.Value.CreatedDate <= endDate)
            .Select(pair => pair.Value)
            .ToList();
        return Task.FromResult(Result<List<User>>.Success(SuccessType.Ok,users));
    }

    public Task<Result<List<User>>> GetUsersForPeriodByUpdatedDate(DateTime startDate, DateTime endDate)
    {
        var users = _users
            .Where(pair => pair.Value.UpdatedDate >= startDate && pair.Value.UpdatedDate <= endDate)
            .Select(pair => pair.Value)
            .ToList();
        return Task.FromResult(Result<List<User>>.Success(SuccessType.Ok,users));
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