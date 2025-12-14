namespace TelegramBotClient.Models;

class UserSession
{
    public LoginState State { get; set; } = LoginState.None;
    public string? Email { get; set; }
    public string? Token { get; set; }
}