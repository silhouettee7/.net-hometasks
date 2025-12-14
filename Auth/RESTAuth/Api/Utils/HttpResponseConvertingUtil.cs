using RESTAuth.Domain.Models;

namespace RESTAuth.Api.Utils;

public class HttpResponseConvertingUtil
{
    public IResult CreateResponse<T>(AppResult<T> appResult)
    {
        if (appResult.IsSuccess)
        {
            if (appResult.SuccessType is null)
            {
                return Results.Problem();
            }
            switch (appResult.SuccessType)
            {
                case SuccessType.Ok:
                    return Results.Ok(appResult.Value);
                case SuccessType.Created:
                    return Results.Created();
                case SuccessType.NoContent:
                    return Results.NoContent();
            }
        }

        return HandleError(appResult);
    }
    
    public IResult CreateResponse(AppResult appResult)
    {
        if (appResult.IsSuccess)
        {
            if (appResult.SuccessType is null)
            {
                return Results.Problem();
            }
            switch (appResult.SuccessType)
            {
                case SuccessType.Ok:
                    return Results.Ok(appResult);
                case SuccessType.Created:
                    return Results.Created();
                case SuccessType.NoContent:
                    return Results.NoContent();
            }
        }
        return HandleError(appResult);
    }

    private IResult HandleError(AppResult appResult)
    {
        if (appResult.AppError is null)
        {
            return Results.Problem();
        }
        switch (appResult.AppError.ErrorType)
        {
            case ErrorType.BadRequest:
                return Results.BadRequest(appResult.AppError.Message);
            case ErrorType.NotFound:
                return Results.NotFound(appResult.AppError.Message);
            case ErrorType.ServerError:
                return Results.Problem(appResult.AppError.Message);
            default:
                return Results.Empty;
        }
    }
}