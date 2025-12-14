using RESTAuth.Domain.Dtos;
using RESTAuth.Domain.Models;

namespace RESTAuth.Domain.Abstractions.Services;

public interface IAuthService
{
    Task<AppResult<LoginResult>> Login(UserLoginDto dto);
    Task<AppResult<TelegramLoginResult>> Login(TelegramLoginDto dto);
}

public class TelegramLoginResult
{
    public long TelegramChatId { get; set; }
    public string Role { get; set; }
}