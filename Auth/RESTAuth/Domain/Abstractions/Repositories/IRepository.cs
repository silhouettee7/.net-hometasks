using System.Linq.Expressions;
using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Domain.Abstractions.Repositories;

public interface IRepository<TEntity, TId> 
    where TEntity : Entity<TId> 
    where TId : struct
{
    Task<AppResult<TId>> Add(TEntity entity);
    Task<AppResult> Update(TEntity entity);
    Task<AppResult> Delete(TId id);
    Task<AppResult<TEntity>> GetEntityById(TId id);
}