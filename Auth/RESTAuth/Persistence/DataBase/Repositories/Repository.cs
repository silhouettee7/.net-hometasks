
using Microsoft.EntityFrameworkCore;
using RESTAuth.Domain.Abstractions.Repositories;
using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Persistence.DataBase.Repositories;

public class Repository<TEntity,TId>(AppDbContext context): IRepository<TEntity,TId> 
    where TId : struct 
    where TEntity : Entity<TId>
{
    protected readonly DbSet<TEntity> dbSet = context.Set<TEntity>();
    public async Task<Result> Add(TEntity entity)
    {
        try
        {
            await dbSet.AddAsync(entity);
            await context.SaveChangesAsync();
            return Result.Success(SuccessType.Created);
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(ErrorType.ServerError, ex.Message));
        }
    }

    public async Task<Result> Update(TEntity entity)
    {
        try
        {
            dbSet.Update(entity);
            await context.SaveChangesAsync();
            return Result.Success(SuccessType.NoContent);
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(ErrorType.ServerError, ex.Message));
        }
    }

    public async Task<Result> Delete(TId id)
    {
        try
        {
            await dbSet
                .Where(e => e.Id.Equals(id))
                .ExecuteDeleteAsync();
            return Result.Success(SuccessType.NoContent);
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(ErrorType.ServerError, ex.Message));
        }
    }

    public async Task<Result<TEntity>> GetEntityById(TId id)
    {
        try
        {
            var entity = await dbSet
                .Where(e => e.Id.Equals(id))
                .SingleOrDefaultAsync();
            return Result<TEntity>.Success(SuccessType.Ok, entity);
        }
        catch (Exception ex)
        {
            return Result<TEntity>.Failure(new Error(ErrorType.ServerError, ex.Message));
        }
    }
}
