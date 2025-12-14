using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Shared.Rabbit.Abstractions;

public abstract class RabbitConfigurer(
    IRabbitMqChannelAccessor accessor,
    IConfiguration configuration,
    string chapter)
{
    protected IChannel? _channel;
    protected readonly string _exchangeName = configuration[$"RabbitMQ:{chapter}:ExchangeName"] ?? "";
    protected readonly string _queueName = configuration[$"RabbitMQ:{chapter}:QueueName"] ?? "";
    protected readonly string _routingKey = configuration[$"RabbitMQ:{chapter}:RoutingKey"] ?? "";
    
    protected virtual async Task ConfigureRabbitAsync(string exchangeType, bool durable)
    {
        _channel = await accessor.GetChannelAsync();
        await _channel.ExchangeDeclareAsync(_exchangeName, exchangeType, durable: durable);
        await _channel.QueueDeclareAsync(_queueName, durable: durable, exclusive:false);
        await _channel.QueueBindAsync(_queueName, _exchangeName, _routingKey);
    }
}