using TickLeadLagAnalyzer.Domain.Models;

namespace TickLeadLagAnalyzer.Domain.Interfaces;

public interface IMt5ConnectionService : IAsyncDisposable
{
    ConnectionStatus Status { get; }
    event EventHandler<ConnectionStatus>? StatusChanged;
    event EventHandler<TickData>? TickReceived;
    
    Task<bool> ConnectAsync(string server, ulong login, string password, CancellationToken ct = default);
    Task DisconnectAsync();
    Task SubscribeAsync(string symbol);
    Task UnsubscribeAsync(string symbol);
    IReadOnlyList<string> GetAvailableSymbols();
    bool SymbolExists(string symbol);
}
