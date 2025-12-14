using Shared.Configuration.Abstractions;

namespace RESTAuth.Api.Workers;

public class InitializerHostedService(IInitializable init): IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await init.InitializeAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}