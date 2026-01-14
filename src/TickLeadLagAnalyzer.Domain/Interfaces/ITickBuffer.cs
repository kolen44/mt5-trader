using TickLeadLagAnalyzer.Domain.Models;

namespace TickLeadLagAnalyzer.Domain.Interfaces;

public interface ITickBuffer
{
    void AddTick(TickData tick);
    IReadOnlyList<TickData> GetTicks(string symbol);
    IReadOnlyList<string> GetSymbols();
    void ClearSymbol(string symbol);
    void Clear();
    void UpdateConfiguration(BufferConfiguration config);
    BufferConfiguration Configuration { get; }
    event EventHandler? BufferUpdated;
}
