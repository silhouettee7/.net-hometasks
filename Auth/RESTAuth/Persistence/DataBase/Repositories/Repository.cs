
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
    public async Task<AppResult<TId>> Add(TEntity entity)
    {
        try
        {
            await dbSet.AddAsync(entity);
            await context.SaveChangesAsync();
            return AppResult<TId>.Success(SuccessType.Created, entity.Id);
        }
        catch (Exception ex)
        {
            return AppResult<TId>.Failure(new AppError(ErrorType.ServerError, ex.Message));
        }
    }

    public async Task<AppResult> Update(TEntity entity)
    {
        try
        {
            dbSet.Update(entity);
            await context.SaveChangesAsync();
            return AppResult.Success(SuccessType.NoContent);
        }
        catch (Exception ex)
        {
            return AppResult.Failure(new AppError(ErrorType.ServerError, ex.Message));
        }
    }

    public async Task<AppResult> Delete(TId id)
    {
        try
        {
            await dbSet
                .Where(e => e.Id.Equals(id))
                .ExecuteDeleteAsync();
            return AppResult.Success(SuccessType.NoContent);
        }
        catch (Exception ex)
        {
            return AppResult.Failure(new AppError(ErrorType.ServerError, ex.Message));
        }
    }

    public async Task<AppResult<TEntity>> GetEntityById(TId id)
    {
        try
        {
            var entity = await dbSet
                .Where(e => e.Id.Equals(id))
                .SingleOrDefaultAsync();
            return AppResult<TEntity>.Success(SuccessType.Ok, entity);
        }
        catch (Exception ex)
        {
            return AppResult<TEntity>.Failure(new AppError(ErrorType.ServerError, ex.Message));
        }
    }
}
