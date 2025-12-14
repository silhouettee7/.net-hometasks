using TelegramBotClient.Abstractions.Bot;

namespace TelegramBotClient.Workers;

public class TelegramBotServiceWorker(ITelegramBotService botService): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await botService.StartAsync(stoppingToken);
        var user = await botService.GeMeAsync(stoppingToken);
        Console.WriteLine($"Bot @{user.Username} started");
    }
}