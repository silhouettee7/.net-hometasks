using Microsoft.EntityFrameworkCore;
using RESTAuth.Domain.Entities;
using RESTAuth.Persistence.DataBase;

namespace RESTAuth.Application.Graph;

public class Query
{
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetUsers([Service] AppDbContext context)
        => context.Users;
    
    public async Task<User?> GetUserById(Guid id, [Service] AppDbContext context)
        => await context.Users
            .FindAsync(id);
}