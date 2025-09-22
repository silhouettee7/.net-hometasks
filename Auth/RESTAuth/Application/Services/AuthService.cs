using RESTAuth.Domain.Abstractions.Repositories;
using RESTAuth.Domain.Abstractions.Services;
using RESTAuth.Domain.Dtos;
using RESTAuth.Domain.Models;

namespace RESTAuth.Application.Services;

public class AuthService(IUserRepository userRepository): IAuthService
{
    public async Task<Result<LoginResult>> Login(UserLoginDto dto)
    {
        var result = await userRepository.GetUserByEmail(dto.Email);
        if (!result.IsSuccess)
        {
            return Result<LoginResult>.Failure(result.Error);
        }

        if (result.Value!.Password != dto.Password)
        {
            return Result<LoginResult>.Failure(new Error(ErrorType.BadRequest, "Invalid password"));
        }
        
        return Result<LoginResult>.Success(new LoginResult
        {
            Id = result.Value.Id, 
            Role = result.Value.Role
        });
    }
}