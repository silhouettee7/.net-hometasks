using System.Collections.Concurrent;
using RESTAuth.Domain.Abstractions.Repositories;
using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Persistence.InMemoryStorage;

public class InMemoryRepository<TEntity, TId>: IRepository<TEntity, TId> 
    where TEntity : Entity<TId> 
    where TId : struct
{
    protected readonly ConcurrentDictionary<TId, TEntity> _data = new();

    public Task<Result> Add(TEntity entity)
    {
        var success = _data.TryAdd(entity.Id, entity);
        if (success)
        {
            return Task.FromResult(Result.Success(SuccessType.Created));
        }
        return Task.FromResult(Result.Failure(new Error(ErrorType.ServerError, "Unable to create new entity")));
    }

    public Task<Result> Update(TEntity entity)
    {
        if (!_data.ContainsKey(entity.Id))
        {
            return Task.FromResult(Result.Failure(new Error(ErrorType.NotFound, "Entity not found")));
        }
        _data[entity.Id] = entity;
        return Task.FromResult(Result.Success(SuccessType.NoContent));
    }

    public Task<Result> Delete(TId id)
    {
        var success = _data.TryRemove(id, out var obj);
        if (success)
        {
            return Task.FromResult(Result.Success(SuccessType.NoContent));
        }
        if (obj is null)
        {
            return Task.FromResult(Result.Failure(new Error(ErrorType.NotFound, "Entity not found")));
        }
        return Task.FromResult(Result.Failure(new Error(ErrorType.ServerError, "Unable to delete entity")));
    }

    public Task<Result<TEntity>> GetEntityById(TId id)
    {
        var success = _data.TryGetValue(id, out var entity);
        if (!success)
        {
            return Task.FromResult(Result<TEntity>.Failure(new Error(ErrorType.NotFound, "Entity not found")));
        }
        return Task.FromResult(Result<TEntity>.Success(SuccessType.Ok,entity!));
    }
}