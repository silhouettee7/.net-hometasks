using Shared.Configuration.Abstractions;
using TelegramBotClient.Rabbit.Consumers;

namespace TelegramBotClient.Workers;

public class ReportNotificationQueueConsumerWorker(
    ReportNotificationQueueConsumer consumer, 
    IInitializable init): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!init.IsInit)
        {
            await Task.Delay(1000, stoppingToken);
        }
        await consumer.ConsumeAsync();
    }
}