using System.Collections.Concurrent;
using RESTAuth.Domain.Entities;

namespace RESTAuth.Persistence.InMemoryStorage;

public class LocalStorage<TEntity, TId>
    where TEntity : Entity<TId> 
    where TId : struct
{
    public readonly ConcurrentDictionary<TId, TEntity> Data = new();
}