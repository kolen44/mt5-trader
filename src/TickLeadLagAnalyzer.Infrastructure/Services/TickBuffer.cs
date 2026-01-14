using System.Collections.Concurrent;
using TickLeadLagAnalyzer.Domain.Interfaces;
using TickLeadLagAnalyzer.Domain.Models;

namespace TickLeadLagAnalyzer.Infrastructure.Services;

public sealed class TickBuffer : ITickBuffer
{
    private readonly ConcurrentDictionary<string, LinkedList<TickData>> _ticksBySymbol = new();
    private readonly object _lock = new();
    private BufferConfiguration _configuration = new();
    
    public BufferConfiguration Configuration => _configuration;
    
    public event EventHandler? BufferUpdated;
    
    public void AddTick(TickData tick)
    {
        var ticks = _ticksBySymbol.GetOrAdd(tick.Symbol, _ => new LinkedList<TickData>());
        
        lock (_lock)
        {
            ticks.AddLast(tick);
            PruneBuffer(tick.Symbol, ticks);
        }
        
        BufferUpdated?.Invoke(this, EventArgs.Empty);
    }
    
    public IReadOnlyList<TickData> GetTicks(string symbol)
    {
        if (!_ticksBySymbol.TryGetValue(symbol, out var ticks))
            return Array.Empty<TickData>();
        
        lock (_lock)
        {
            return ticks.ToList();
        }
    }
    
    public IReadOnlyList<string> GetSymbols()
    {
        return _ticksBySymbol.Keys.ToList();
    }
    
    public void ClearSymbol(string symbol)
    {
        if (_ticksBySymbol.TryGetValue(symbol, out var ticks))
        {
            lock (_lock)
            {
                ticks.Clear();
            }
        }
        _ticksBySymbol.TryRemove(symbol, out _);
        BufferUpdated?.Invoke(this, EventArgs.Empty);
    }
    
    public void Clear()
    {
        foreach (var symbol in _ticksBySymbol.Keys.ToList())
        {
            ClearSymbol(symbol);
        }
        _ticksBySymbol.Clear();
        BufferUpdated?.Invoke(this, EventArgs.Empty);
    }
    
    public void UpdateConfiguration(BufferConfiguration config)
    {
        _configuration = config;
        
        // Prune all buffers with new configuration
        lock (_lock)
        {
            foreach (var kvp in _ticksBySymbol)
            {
                PruneBuffer(kvp.Key, kvp.Value);
            }
        }
        
        BufferUpdated?.Invoke(this, EventArgs.Empty);
    }
    
    private void PruneBuffer(string symbol, LinkedList<TickData> ticks)
    {
        var cutoffTime = DateTime.UtcNow.AddSeconds(-_configuration.WindowDurationSeconds);
        
        // Remove old ticks by time
        while (ticks.First != null && ticks.First.Value.Timestamp < cutoffTime)
        {
            ticks.RemoveFirst();
        }
        
        // Remove excess ticks by count
        while (ticks.Count > _configuration.MaxTicksPerSymbol)
        {
            ticks.RemoveFirst();
        }
    }
}
