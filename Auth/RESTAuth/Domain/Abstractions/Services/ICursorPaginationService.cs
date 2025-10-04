using RESTAuth.Domain.Abstractions.Repositories;
using RESTAuth.Domain.Dtos;
using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Domain.Abstractions.Services;

public interface ICursorPaginationService<TEntity,TId> 
    where TEntity : Entity<TId> 
    where TId : struct, IComparable<TId>
{
    Task<Result<CursorPaginationResponse<TEntity>>> GetPageAsync(IQueryBuilder<TEntity,TId> queryBuilder,CursorPaginationRequest request);
}