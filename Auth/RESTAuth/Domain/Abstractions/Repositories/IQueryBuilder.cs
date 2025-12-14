using System.Linq.Expressions;
using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Domain.Abstractions.Repositories;

public interface IQueryBuilder<TEntity, in TId> 
    where TEntity : Entity<TId> 
    where TId : struct, IComparable<TId>
{
    IQueryBuilder<TEntity,TId> OrderBy<TKey>(Expression<Func<TEntity, TKey>> expression, bool ascending);
    IQueryBuilder<TEntity, TId> Where(Expression<Func<TEntity, bool>> predicate);
    IQueryBuilder<TEntity,TId> Take(int count);
    Task<AppResult<List<TEntity>>> ExecuteQuery();
}