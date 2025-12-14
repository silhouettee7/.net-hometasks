using RESTAuth.Application.Rabbit.Consumers;

namespace RESTAuth.Api.Workers;

public class ReportQueueConsumerBackgroundService(
    IServiceScopeFactory serviceScopeFactory
    ): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sp = serviceScopeFactory.CreateScope().ServiceProvider;
        var consumer = sp.GetRequiredService<ReportQueueConsumer>();
        await consumer.ConsumeAsync();
    }
}