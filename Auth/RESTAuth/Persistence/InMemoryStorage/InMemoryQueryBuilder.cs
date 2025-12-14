using System.Collections.Concurrent;
using System.Linq.Expressions;
using RESTAuth.Domain.Abstractions.Repositories;
using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Persistence.InMemoryStorage;

public class InMemoryQueryBuilder<TEntity, TId>(LocalStorage<TEntity,TId> storage): IQueryBuilder<TEntity, TId> 
    where TEntity : Entity<TId> 
    where TId : struct, IComparable<TId>
{
    private IQueryable<TEntity> _query = storage.Data.Values.AsQueryable();

    public IQueryBuilder<TEntity, TId> OrderBy<TKey>(Expression<Func<TEntity, TKey>> expression, bool ascending)
    {
        _query = ascending ? _query.OrderBy(expression): _query.OrderByDescending(expression);
        return this;
    }

    public IQueryBuilder<TEntity, TId> Where(Expression<Func<TEntity, bool>> predicate)
    {
        _query = _query.Where(predicate);
        return this;
    }

    public IQueryBuilder<TEntity, TId> Take(int count)
    {
        _query = _query.Take(count);
        return this;
    }

    public Task<AppResult<List<TEntity>>> ExecuteQuery()
    {
        try
        {
            return Task.FromResult(AppResult<List<TEntity>>.Success(SuccessType.Ok, _query.ToList()));
        }
        catch (Exception ex)
        {
            return Task.FromResult(AppResult<List<TEntity>>.Failure(new AppError(ErrorType.ServerError, ex.Message)));
        }
    }
}