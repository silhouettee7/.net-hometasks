using Shared.Configuration.Abstractions;

namespace TelegramBotClient.Workers;

public class InitializerWorker(IInitializable init): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await init.InitializeAsync();
        init.IsInit = true;
    }
}