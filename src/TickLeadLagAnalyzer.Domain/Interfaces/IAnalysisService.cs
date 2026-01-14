using TickLeadLagAnalyzer.Domain.Models;

namespace TickLeadLagAnalyzer.Domain.Interfaces;

public interface IAnalysisService
{
    Dictionary<string, double[]> CalculateCumulativeReturns(ITickBuffer buffer);
    double CalculateGapToBase(string symbol, string baseSymbol, ITickBuffer buffer);
    List<GapChartPoint> CalculateGapChartData(string baseSymbol, ITickBuffer buffer);
    
    LagCorrelationResult CalculateLagCorrelation(
        string symbol, 
        string baseSymbol, 
        ITickBuffer buffer,
        int lagRangeSeconds = 5,
        int lagStepMs = 100);
    
    List<LagCorrelationResult> CalculateAllLagCorrelations(
        string baseSymbol,
        ITickBuffer buffer,
        int lagRangeSeconds = 5,
        int lagStepMs = 100);
}
