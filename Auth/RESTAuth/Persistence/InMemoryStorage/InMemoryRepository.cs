using System.Collections.Concurrent;
using RESTAuth.Domain.Abstractions.Repositories;
using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Persistence.InMemoryStorage;

public class InMemoryRepository<TEntity, TId>(LocalStorage<TEntity, TId> storage): IRepository<TEntity, TId> 
    where TEntity : Entity<TId> 
    where TId : struct
{
    protected ConcurrentDictionary<TId, TEntity> _data => storage.Data;

    public Task<AppResult<TId>> Add(TEntity entity)
    {
        var success = _data.TryAdd(entity.Id, entity);
        if (success)
        {
            return Task.FromResult(AppResult<TId>.Success(SuccessType.Created, entity.Id));
        }
        return Task.FromResult(AppResult<TId>.Failure(new AppError(ErrorType.ServerError, "Unable to create new entity")));
    }

    public Task<AppResult> Update(TEntity entity)
    {
        if (!_data.ContainsKey(entity.Id))
        {
            return Task.FromResult(AppResult.Failure(new AppError(ErrorType.NotFound, "Entity not found")));
        }
        _data[entity.Id] = entity;
        return Task.FromResult(AppResult.Success(SuccessType.NoContent));
    }

    public Task<AppResult> Delete(TId id)
    {
        var success = _data.TryRemove(id, out var obj);
        if (success)
        {
            return Task.FromResult(AppResult.Success(SuccessType.NoContent));
        }
        if (obj is null)
        {
            return Task.FromResult(AppResult.Failure(new AppError(ErrorType.NotFound, "Entity not found")));
        }
        return Task.FromResult(AppResult.Failure(new AppError(ErrorType.ServerError, "Unable to delete entity")));
    }

    public Task<AppResult<TEntity>> GetEntityById(TId id)
    {
        var success = _data.TryGetValue(id, out var entity);
        if (!success)
        {
            return Task.FromResult(AppResult<TEntity>.Failure(new AppError(ErrorType.NotFound, "Entity not found")));
        }
        return Task.FromResult(AppResult<TEntity>.Success(SuccessType.Ok,entity!));
    }
}