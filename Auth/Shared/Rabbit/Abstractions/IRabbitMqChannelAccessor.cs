using RabbitMQ.Client;

namespace Shared.Rabbit.Abstractions;

public interface IRabbitMqChannelAccessor : IAsyncDisposable
{
    Task<IChannel> GetChannelAsync();
}