namespace TelegramBotClient.Models;

enum LoginState
{
    None,
    WaitingEmail,
    WaitingPassword,
    LoggedIn
}