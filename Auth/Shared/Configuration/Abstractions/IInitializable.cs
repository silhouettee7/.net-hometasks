namespace Shared.Configuration.Abstractions;

public interface IInitializable
{
    Task InitializeAsync();
    bool IsInit { get; set; }
}