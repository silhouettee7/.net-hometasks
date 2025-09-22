using RESTAuth.Domain.Models;

namespace RESTAuth.Api.Utils;

public class HttpResponseCreator
{
    public IResult CreateResponse<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }
        return HandleError(result);
    }
    
    public IResult CreateResponse(Result result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok();
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