using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Shared.Configuration.Abstractions;
using Shared.Rabbit.Abstractions;

namespace Shared.Rabbit.Utils;

public class RabbitMqChannelAccessor: IRabbitMqChannelAccessor, IInitializable
{
    private readonly ILogger<RabbitMqChannelAccessor> _logger;
    private readonly ConnectionFactory _factory;
    private IConnection _connection;
    private IChannel? _channel;
    private readonly string _appName;
    private SemaphoreSlim _channelLock = new (1);
    
    public RabbitMqChannelAccessor(IConfiguration configuration, 
        ILogger<RabbitMqChannelAccessor> logger)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
            UserName = configuration["RabbitMQ:UserName"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672")
        };
        _appName = configuration["RabbitMQ:AppName"] ?? Guid.NewGuid().ToString();
        _logger = logger;
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _connection = await _factory.CreateConnectionAsync(_appName);
        _connection.ConnectionShutdownAsync += (_, ea) =>
        {
            _logger.LogWarning("RabbitMQ connection shutdown: {Reason}", ea.ReplyText);
            return Task.CompletedTask;
        };
    }

    public bool IsInit { get; set; }

    public async Task<IChannel> GetChannelAsync()
    {
        await _channelLock.WaitAsync();
        try
        {
            if (_channel is not null && _channel.IsOpen)
            {
                return _channel;
            }

            _channel = await _connection.CreateChannelAsync();
            _channel.ChannelShutdownAsync += (_, ea) =>
            {
                _logger.LogError("RabbitMQ channel shutdown: {Reason}", ea.ReplyText);
                return Task.CompletedTask;
            };
        }
        finally
        {
            _channelLock.Release();
        }
        return _channel;
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.CloseAsync();
        await _connection.DisposeAsync();
    }
}