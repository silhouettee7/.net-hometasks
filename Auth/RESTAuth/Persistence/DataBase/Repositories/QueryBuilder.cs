using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RESTAuth.Domain.Abstractions.Repositories;
using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Persistence.DataBase.Repositories;

public class QueryBuilder<TEntity, TId>(AppDbContext context): IQueryBuilder<TEntity, TId> 
    where TEntity : Entity<TId> 
    where TId : struct, IComparable<TId>
{
    private IQueryable<TEntity> dbSet = context.Set<TEntity>();
    public IQueryBuilder<TEntity, TId> OrderBy<TKey>(Expression<Func<TEntity, TKey>> expression, bool ascending)
    {
        if (ascending)
        {
            dbSet = dbSet.OrderBy(expression);
        }
        else
        {
            dbSet = dbSet.OrderByDescending(expression);
        }
        return this;
    }

    public IQueryBuilder<TEntity, TId> Where(Expression<Func<TEntity, bool>> predicate)
    {
        dbSet = dbSet.Where(predicate);
        return this;
    }

    public IQueryBuilder<TEntity, TId> Take(int count)
    {
        dbSet = dbSet.Take(count);
        return this;
    }

    public async Task<AppResult<List<TEntity>>> ExecuteQuery()
    {
        try
        {
            var result = await dbSet.ToListAsync();
            return AppResult<List<TEntity>>.Success(SuccessType.Ok, result);
        }
        catch (Exception ex)
        {
            return AppResult<List<TEntity>>.Failure(new AppError(ErrorType.ServerError, ex.Message));
        }
    }
}