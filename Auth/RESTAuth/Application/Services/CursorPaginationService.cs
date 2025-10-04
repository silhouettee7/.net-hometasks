using System.ComponentModel;
using System.Text;
using RESTAuth.Domain.Abstractions.Repositories;
using RESTAuth.Domain.Abstractions.Services;
using RESTAuth.Domain.Dtos;
using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Application.Services;

public class CursorPaginationService<TEntity, TId>: ICursorPaginationService<TEntity, TId> 
    where TEntity : Entity<TId> 
    where TId : struct, IComparable<TId>
{
    public async Task<Result<CursorPaginationResponse<TEntity>>> GetPageAsync(IQueryBuilder<TEntity,TId> queryBuilder, CursorPaginationRequest request)
    {
        return request.Cursor == null 
            ? GetCursorPaginationResponse(await HandleFirstPage(request, queryBuilder)) 
            : GetCursorPaginationResponse(await HandlePage(request, queryBuilder));
    }
    
    private async Task<Result<List<TEntity>>> HandleFirstPage(
        CursorPaginationRequest request, IQueryBuilder<TEntity,TId> queryBuilder)
    {
        var result = await queryBuilder
            .OrderBy(x => x.Id, true)
            .Take(request.PageSize)
            .ExecuteQuery();
        return result;
    }

    private async Task<Result<List<TEntity>>> HandlePage(
        CursorPaginationRequest request, IQueryBuilder<TEntity,TId> queryBuilder)
    {
        queryBuilder.OrderBy(x => x.Id, true);
        var cursor = DecodeCursor(request.Cursor!);
        var id = cursor.Id;
        if (request.Forward)
        {
            queryBuilder
                .Where(x => x.Id.CompareTo(id) > 0);
        }
        else
        {
            queryBuilder
                .Where(x => x.Id.CompareTo(id) < 0)
                .OrderBy(x => x.Id, false);
        }
        return await queryBuilder
            .Take(request.PageSize)
            .ExecuteQuery();
    }
    private Result<CursorPaginationResponse<TEntity>> GetCursorPaginationResponse(Result<List<TEntity>> result)
    {
        if (!result.IsSuccess)
        {
            return Result<CursorPaginationResponse<TEntity>>.Failure(result.Error!);
        }
        var entities = result.Value!;
        var firstElem = entities.FirstOrDefault();
        var lastElem = entities.LastOrDefault();
        var response = new CursorPaginationResponse<TEntity>
        {
            Items = entities,
            NextCursor = firstElem is null ? null : EncodeCursor(new Cursor { Id = firstElem.Id }),
            PreviousCursor = lastElem is null ? null : EncodeCursor(new Cursor { Id = lastElem.Id }),
            HasNext = lastElem != null,
            HasPrevious = firstElem != null
        };
        return Result<CursorPaginationResponse<TEntity>>.Success(SuccessType.Ok, response);
    }
    
    private Cursor DecodeCursor(string cursor)
    {
        var stringCursor = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
        var converter = TypeDescriptor.GetConverter(typeof(TId));
        if (converter.CanConvertFrom(typeof(string)))
        {
            try
            {
                return new Cursor
                {
                    Id = (TId)converter.ConvertFrom(stringCursor)!
                };
            }
            catch (Exception)
            {
                throw new InvalidCastException("Cannot convert");
            }
        }
        throw new InvalidCastException("No valid cursor found");
    }

    private string? EncodeCursor(Cursor cursor)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(cursor.Id.ToString() ?? string.Empty));
    }
    
    private class Cursor
    {
        public TId Id { get; set; }
    }
}