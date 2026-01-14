namespace TickLeadLagAnalyzer.Domain.Models;

public sealed record GapChartPoint
{
    public required string Symbol { get; init; }
    public required DateTime Timestamp { get; init; }
    public required double GapToBase { get; init; }
}
