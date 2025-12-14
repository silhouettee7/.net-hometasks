using Telegram.Bot.Types;

namespace TelegramBotClient.Abstractions.Bot;

public interface ITelegramBotService
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task SendMessageAsync(long chatId, string message, CancellationToken cancellationToken = default);
    Task<User> GeMeAsync(CancellationToken cancellationToken = default);
    Task SendReportLinkAsync(long chatId, string link, CancellationToken cancellationToken = default);
}