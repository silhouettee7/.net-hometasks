using RESTAuth.Domain.Dtos;
using RESTAuth.Domain.Models;

namespace RESTAuth.Domain.Abstractions.Services;

public interface IAuthService
{
    Task<Result<LoginResult>> Login(UserLoginDto dto);
}