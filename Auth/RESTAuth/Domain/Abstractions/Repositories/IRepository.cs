using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Domain.Abstractions.Repositories;

public interface IRepository<TEntity, in TId> 
    where TEntity : Entity<TId> 
    where TId : struct
{
    Task<Result> Add(TEntity entity);
    Task<Result> Update(TEntity entity);
    Task<Result> Delete(TId id);
    Task<Result<TEntity>> GetEntityById(TId id);
}