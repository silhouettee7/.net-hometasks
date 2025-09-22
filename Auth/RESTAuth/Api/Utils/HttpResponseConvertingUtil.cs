using RESTAuth.Domain.Models;

namespace RESTAuth.Api.Utils;

public class HttpResponseConvertingUtil
{
    public IResult CreateResponse<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            if (result.SuccessType is null)
            {
                return Results.Problem();
            }
            switch (result.SuccessType)
            {
                case SuccessType.Ok:
                    return Results.Ok(result.Value);
                case SuccessType.Created:
                    return Results.Created();
                case SuccessType.NoContent:
                    return Results.NoContent();
            }
        }

        return HandleError(result);
    }
    
    public IResult CreateResponse(Result result)
    {
        if (result.IsSuccess)
        {
            if (result.SuccessType is null)
            {
                return Results.Problem();
            }
            switch (result.SuccessType)
            {
                case SuccessType.Ok:
                    return Results.Ok(result);
                case SuccessType.Created:
                    return Results.Created();
                case SuccessType.NoContent:
                    return Results.NoContent();
            }
        }
        return HandleError(result);
    }

    private IResult HandleError(Result result)
    {
        if (result.Error is null)
        {
            return Results.Problem();
        }
        switch (result.Error.ErrorType)
        {
            case ErrorType.BadRequest:
                return Results.BadRequest(result.Error.Message);
            case ErrorType.NotFound:
                return Results.NotFound(result.Error.Message);
            case ErrorType.ServerError:
                return Results.Problem(result.Error.Message);
            default:
                return Results.Empty;
        }
    }
}