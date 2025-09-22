namespace RESTAuth.Domain.Models;

public enum ErrorType
{
    AuthenticationError,
    AuthorizationError,
    BadRequest,
    NotFound,
    ServerError,
}