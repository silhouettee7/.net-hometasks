using Microsoft.EntityFrameworkCore;
using RESTAuth.Domain.Abstractions.Repositories;
using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Persistence.DataBase.Repositories;

public class UserRepository(AppDbContext context) : Repository<User,Guid>(context), IUserRepository
{
    public async Task<AppResult<Dictionary<string, decimal>>> GetUserAverageSalariesByDepartment()
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
            return AppResult<Dictionary<string, decimal>>.Success(SuccessType.Ok, result);
        }
        catch (Exception ex)
        {
            return AppResult<Dictionary<string, decimal>>.Failure(new AppError(ErrorType.ServerError, ex.Message));
        }
    }

    public async Task<AppResult<User>> GetUserByEmail(string email)
    {
        try
        {
            var user = await dbSet.SingleOrDefaultAsync(u => u.Email == email);
            return AppResult<User>.Success(SuccessType.Ok, user);
        }
        catch (Exception ex)
        {
            return AppResult<User>.Failure(new AppError(ErrorType.ServerError, ex.Message));
        }
    }

    public async Task CreateUsers(IEnumerable<User> users)
    {
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }

    public async Task<List<UserReport>> GetAllUsers()
    {
        return await context.Users.Select(u => new UserReport
        {
            Name = u.Name,
            Email = u.Email,
            Salary = u.Salary,
            Department = u.Department,
            CreatedDate = u.CreatedDate,
            UpdatedDate = u.UpdatedDate
        }).ToListAsync();
    }
}