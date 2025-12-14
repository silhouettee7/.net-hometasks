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
    public async Task<AppResult<CursorPaginationResponse<TEntity>>> GetPageAsync(IQueryBuilder<TEntity,TId> queryBuilder, CursorPaginationRequest request)
    {
        return request.Cursor == null 
            ? await HandleFirstPage(request, queryBuilder)
            : await HandleOtherPage(request, queryBuilder);
    }
    
    private async Task<AppResult<CursorPaginationResponse<TEntity>>> HandleFirstPage(
        CursorPaginationRequest request, IQueryBuilder<TEntity,TId> queryBuilder)
    {
        var dataResult = await queryBuilder
            .OrderBy(x => x.Id, request.Forward)
            .Take(request.PageSize+1)
            .ExecuteQuery();
        if (!dataResult.IsSuccess)
        {
            return AppResult<CursorPaginationResponse<TEntity>>.Failure(dataResult.AppError!);
        }
        var entities = dataResult.Value!;
        var additionalElem = entities
            .Skip(request.PageSize)
            .FirstOrDefault();
        var response = new CursorPaginationResponse<TEntity>
        {
            Items = additionalElem is null ? entities: entities
                .Take(request.PageSize)
                .ToList()
        };
        if (additionalElem is null)
        {
            return AppResult<CursorPaginationResponse<TEntity>>.Success(SuccessType.Ok, response);
        }
        if (request.Forward)
        {
            response.HasNext = true;
            response.NextCursor = EncodeCursor(new Cursor {Id = additionalElem.Id});
        }
        else
        {
            response.HasPrevious = true;
            response.PreviousCursor = EncodeCursor(new Cursor {Id = additionalElem.Id});
        }
        return AppResult<CursorPaginationResponse<TEntity>>.Success(SuccessType.Ok, response);
    }

    private async Task<AppResult<CursorPaginationResponse<TEntity>>> HandleOtherPage(
        CursorPaginationRequest request, IQueryBuilder<TEntity,TId> queryBuilder)
    {
        queryBuilder.OrderBy(x => x.Id, true);
        var cursor = DecodeCursor(request.Cursor!);
        var id = cursor.Id;
        if (request.Forward)
        {
            queryBuilder
                .Where(x => x.Id.CompareTo(id) >= 0);
        }
        else
        {
            queryBuilder
                .Where(x => x.Id.CompareTo(id) <= 0)
                .OrderBy(x => x.Id, false);
        }
        var dataResult = await queryBuilder
            .Take(request.PageSize + 1)
            .ExecuteQuery();
        if (!dataResult.IsSuccess)
        {
            return AppResult<CursorPaginationResponse<TEntity>>.Failure(dataResult.AppError!);
        }
        var entities = dataResult.Value!;
        var additionalElem = entities
            .Skip(request.PageSize)
            .FirstOrDefault();
        var response = new CursorPaginationResponse<TEntity>
        {
            Items = additionalElem is null ? entities: entities
                .Take(request.PageSize)
                .ToList()
        };
        if (request.Forward)
        {
            response.HasPrevious = true;
            response.PreviousCursor = request.Cursor;
            response.HasNext = additionalElem is not null;
            response.NextCursor = response.HasNext ? EncodeCursor(new Cursor { Id = additionalElem!.Id }) : null;
        }
        else
        {
            response.HasNext = true;
            response.NextCursor = request.Cursor;
            response.HasPrevious = additionalElem is not null;
            response.PreviousCursor = response.HasPrevious ? EncodeCursor(new Cursor { Id = additionalElem!.Id }) : null;
        }

        return AppResult<CursorPaginationResponse<TEntity>>.Success(SuccessType.Ok, response);
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

    private string EncodeCursor(Cursor cursor)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(cursor.Id.ToString() ?? string.Empty));
    }
    
    private class Cursor
    {
        public TId Id { get; set; }
    }
}