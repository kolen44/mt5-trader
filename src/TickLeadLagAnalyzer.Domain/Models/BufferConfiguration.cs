namespace TickLeadLagAnalyzer.Domain.Models;

public sealed class BufferConfiguration
{
    public int WindowDurationSeconds { get; set; } = 60;
    public int MaxTicksPerSymbol { get; set; } = 2000;
    public int LagRangeSeconds { get; set; } = 5;
    public int LagStepMilliseconds { get; set; } = 100;
}
