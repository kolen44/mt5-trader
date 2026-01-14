using Microsoft.Extensions.Logging;
using mtapi.mt5;
using TickLeadLagAnalyzer.Domain.Interfaces;
using TickLeadLagAnalyzer.Domain.Models;

namespace TickLeadLagAnalyzer.Infrastructure.Services;

public sealed class Mt5ConnectionService : IMt5ConnectionService
{
    private readonly ILogger<Mt5ConnectionService> _logger;
    private MT5API? _api;
    private ConnectionStatus _status = ConnectionStatus.Disconnected;
    private readonly HashSet<string> _subscribedSymbols = new();
    private readonly object _lock = new();
    
    public ConnectionStatus Status
    {
        get => _status;
        private set
        {
            if (_status != value)
            {
                _status = value;
                StatusChanged?.Invoke(this, value);
            }
        }
    }
    
    public event EventHandler<ConnectionStatus>? StatusChanged;
    public event EventHandler<TickData>? TickReceived;
    
    public Mt5ConnectionService(ILogger<Mt5ConnectionService> logger)
    {
        _logger = logger;
    }
    
    public async Task<bool> ConnectAsync(string server, ulong login, string password, CancellationToken ct = default)
    {
        try
        {
            Status = ConnectionStatus.Connecting;
            _logger.LogInformation("Connecting to MT5 server {Server} with login {Login}", server, login);
            
            // Parse server address
            var (host, port) = ParseServerAddress(server);
            
            _api = new MT5API(login, password, host, port);
            _api.OnQuote += OnQuoteReceived;
            _api.OnConnectProgress += OnConnectProgress;
            
            // Connect asynchronously
            await Task.Run(() =>
            {
                _api.Connect();
            }, ct);
            
            // Wait for connection to be established
            var timeout = DateTime.Now.AddSeconds(30);
            while (!_api.Connected && DateTime.Now < timeout)
            {
                await Task.Delay(100, ct);
                if (ct.IsCancellationRequested)
                {
                    Status = ConnectionStatus.Disconnected;
                    return false;
                }
            }
            
            if (_api.Connected)
            {
                Status = ConnectionStatus.Connected;
                _logger.LogInformation("Successfully connected to MT5 server");
                
                // Log available symbols
                var symbols = GetAvailableSymbols();
                _logger.LogInformation("Available symbols ({Count}): {Symbols}", symbols.Count, string.Join(", ", symbols.Take(20)));
                
                return true;
            }
            else
            {
                Status = ConnectionStatus.Error;
                _logger.LogWarning("Failed to connect to MT5 server - timeout");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to MT5 server");
            Status = ConnectionStatus.Error;
            return false;
        }
    }
    
    private (string host, int port) ParseServerAddress(string server)
    {
        var parts = server.Split(':');
        if (parts.Length == 2 && int.TryParse(parts[1], out int port))
        {
            return (parts[0], port);
        }
        return (server, 443);
    }
    
    private void OnConnectProgress(MT5API sender, ConnectEventArgs args)
    {
        _logger.LogDebug("Connect progress: {Progress}", args.Progress);
    }
    
    private void OnQuoteReceived(MT5API api, Quote quote)
    {
        try
        {
            var tickData = new TickData
            {
                Symbol = quote.Symbol,
                Bid = quote.Bid,
                Ask = quote.Ask,
                Timestamp = DateTime.UtcNow
            };
            
            TickReceived?.Invoke(this, tickData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing quote for {Symbol}", quote.Symbol);
        }
    }
    
    public async Task DisconnectAsync()
    {
        try
        {
            Status = ConnectionStatus.Disconnecting;
            _logger.LogInformation("Disconnecting from MT5 server");
            
            if (_api != null)
            {
                _api.OnQuote -= OnQuoteReceived;
                _api.OnConnectProgress -= OnConnectProgress;
                
                await Task.Run(() =>
                {
                    try
                    {
                        _api.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error during disconnect");
                    }
                });
            }
            
            lock (_lock)
            {
                _subscribedSymbols.Clear();
            }
            
            Status = ConnectionStatus.Disconnected;
            _logger.LogInformation("Disconnected from MT5 server");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from MT5 server");
            Status = ConnectionStatus.Disconnected;
        }
    }
    
    public async Task SubscribeAsync(string symbol)
    {
        if (_api == null || !_api.Connected)
        {
            _logger.LogWarning("Cannot subscribe - not connected");
            return;
        }
        
        try
        {
            // Check if symbol exists, try variations
            var actualSymbol = FindSymbol(symbol);
            if (actualSymbol == null)
            {
                _logger.LogWarning("Symbol {Symbol} not found on server", symbol);
                return;
            }
            
            _logger.LogInformation("Subscribing to symbol {Symbol} (actual: {ActualSymbol})", symbol, actualSymbol);
            
            await Task.Run(() =>
            {
                _api.Subscribe(actualSymbol);
            });
            
            lock (_lock)
            {
                _subscribedSymbols.Add(actualSymbol);
            }
            
            _logger.LogInformation("Subscribed to symbol {Symbol}", actualSymbol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to symbol {Symbol}", symbol);
        }
    }
    
    private string? FindSymbol(string symbol)
    {
        if (_api == null) return null;
        
        // Try exact match first
        if (_api.Symbols.Exist(symbol))
            return symbol;
        
        // Try with common suffixes
        var suffixes = new[] { "", ".m", ".a", ".r", "-m", "_m", ".i", ".pro" };
        foreach (var suffix in suffixes)
        {
            var trySymbol = symbol + suffix;
            if (_api.Symbols.Exist(trySymbol))
                return trySymbol;
        }
        
        // Try to find symbol containing the base name
        try
        {
            var allSymbols = _api.Symbols.Names;
            var match = allSymbols.FirstOrDefault(s => 
                s.StartsWith(symbol, StringComparison.OrdinalIgnoreCase) ||
                s.Replace(".", "").Replace("-", "").Replace("_", "")
                    .Equals(symbol, StringComparison.OrdinalIgnoreCase));
            return match;
        }
        catch
        {
            return null;
        }
    }
    
    public async Task UnsubscribeAsync(string symbol)
    {
        if (_api == null)
            return;
        
        try
        {
            _logger.LogInformation("Unsubscribing from symbol {Symbol}", symbol);
            
            await Task.Run(() =>
            {
                _api.Unsubscribe(symbol);
            });
            
            lock (_lock)
            {
                _subscribedSymbols.Remove(symbol);
            }
            
            _logger.LogInformation("Unsubscribed from symbol {Symbol}", symbol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from symbol {Symbol}", symbol);
        }
    }
    
    public IReadOnlyList<string> GetAvailableSymbols()
    {
        if (_api == null || !_api.Connected)
            return Array.Empty<string>();
        
        try
        {
            return _api.Symbols.Names.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available symbols");
            return Array.Empty<string>();
        }
    }
    
    public bool SymbolExists(string symbol)
    {
        if (_api == null || !_api.Connected)
            return false;
        
        try
        {
            return _api.Symbols.Exist(symbol);
        }
        catch
        {
            return false;
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        _api = null;
    }
}
