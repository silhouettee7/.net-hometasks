using RESTAuth.Domain.Abstractions.Repositories;
using RESTAuth.Domain.Abstractions.Services;
using RESTAuth.Domain.Dtos;
using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Application.Services;

public class AuthService(IUserRepository userRepository): IAuthService
{
    public async Task<AppResult<LoginResult>> Login(UserLoginDto dto)
    {
        return await LoginBaseAsync(dto, userResult => 
            AppResult<LoginResult>.Success(SuccessType.Ok, new LoginResult 
            { 
                Id = userResult!.Value.Id, 
                Role = userResult.Value.Role 
            }));
    }

    public async Task<AppResult<TelegramLoginResult>> Login(TelegramLoginDto dto)
    {
        return await LoginBaseAsync(dto, userResult =>
            AppResult<TelegramLoginResult>.Success(SuccessType.Ok, new TelegramLoginResult
            {
                TelegramChatId = dto.TelegramChatId,
                Role = userResult!.Value.Role
            }));
    }

    private async Task<AppResult<TOut>> LoginBaseAsync<TDto,TOut>(TDto dto,
        Func<AppResult<User>, AppResult<TOut>> success) 
        where TDto: AbstractLoginDto
    {
        var result = await userRepository.GetUserByEmail(dto.Email);
        if (!result.IsSuccess)
        {
            return AppResult<TOut>.Failure(result.AppError);
        }
        if (result.Value!.Password != dto.Password)
        {
            return AppResult<TOut>.Failure(new AppError(ErrorType.BadRequest, "Invalid password"));
        }

        return success(result);
    }
}