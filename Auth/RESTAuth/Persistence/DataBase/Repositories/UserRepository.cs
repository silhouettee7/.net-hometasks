using Microsoft.EntityFrameworkCore;
using RESTAuth.Domain.Abstractions.Repositories;
using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Persistence.DataBase.Repositories;

public class UserRepository(AppDbContext context) : Repository<User,Guid>(context), IUserRepository
{
    public async Task<Result<Dictionary<string, decimal>>> GetUserAverageSalariesByDepartment()
    {
        try
        {
            var result = await dbSet
                .GroupBy(u => u.Department)
                .Select(g => new
                {
                    Key = g.Key,
                    AverageSalary = g.Average(s => s.Salary)
                })
                .ToDictionaryAsync(k => k.Key, v => v.AverageSalary);
            return Result<Dictionary<string, decimal>>.Success(SuccessType.Ok, result);
        }
        catch (Exception ex)
        {
            return Result<Dictionary<string, decimal>>.Failure(new Error(ErrorType.ServerError, ex.Message));
        }
    }

    public async Task<Result<User>> GetUserByEmail(string email)
    {
        try
        {
            var user = await dbSet.SingleOrDefaultAsync(u => u.Email == email);
            return Result<User>.Success(SuccessType.Ok, user);
        }
        catch (Exception ex)
        {
            return Result<User>.Failure(new Error(ErrorType.ServerError, ex.Message));
        }
    }

    public async Task CreateUsers(IEnumerable<User> users)
    {
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }
}