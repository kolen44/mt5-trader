namespace TickLeadLagAnalyzer.Domain.Models;

public sealed record TickData
{
    public required string Symbol { get; init; }
    public required double Bid { get; init; }
    public required double Ask { get; init; }
    public required DateTime Timestamp { get; init; }
    
    public double MidPrice => (Bid + Ask) / 2.0;
    public double Spread => Ask - Bid;
}
