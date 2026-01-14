namespace TickLeadLagAnalyzer.Domain.Models;

public sealed class SymbolState
{
    public required string Symbol { get; init; }
    public double Bid { get; set; }
    public double Ask { get; set; }
    public double Spread => Ask - Bid;
    public double MidPrice => (Bid + Ask) / 2.0;
    public DateTime LastTickTime { get; set; }
    public double CumulativeReturn { get; set; }
    public double GapToBase { get; set; }
    public bool IsBaseSymbol { get; set; }
}
