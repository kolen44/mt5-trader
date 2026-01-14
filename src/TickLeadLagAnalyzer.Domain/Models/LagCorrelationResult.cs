namespace TickLeadLagAnalyzer.Domain.Models;

public sealed record LagCorrelationResult
{
    public required string Symbol { get; init; }
    public double BestLagSeconds { get; init; }
    public double BestCorrelation { get; init; }
    public bool IsLeading => BestLagSeconds > 0;
    public bool IsSignificant => Math.Abs(BestCorrelation) >= 0.5;
}
