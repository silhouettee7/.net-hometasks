using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Shared.Rabbit.Abstractions;
using Shared.Rabbit.Models;

namespace RESTAuth.Application.Rabbit.Producers;

public class ReportQueueProducer(
    IRabbitMqChannelAccessor accessor, 
    IConfiguration configuration): RabbitConfigurer(accessor, configuration,"Reports")
{
    public async Task EnqueueReportAsync(ReportRequest request)
    {
        await ConfigureRabbitAsync(ExchangeType.Direct, true);
        var json = JsonSerializer.Serialize(request);
        var body = Encoding.UTF8.GetBytes(json);

        await _channel!.BasicPublishAsync(
            exchange: _exchangeName,
            routingKey: _routingKey,
            body: body);
    }
}