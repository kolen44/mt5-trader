using TickLeadLagAnalyzer.Domain.Interfaces;
using TickLeadLagAnalyzer.Domain.Models;

namespace TickLeadLagAnalyzer.Infrastructure.Services;

public sealed class AnalysisService : IAnalysisService
{
    public Dictionary<string, double[]> CalculateCumulativeReturns(ITickBuffer buffer)
    {
        var result = new Dictionary<string, double[]>();
        
        foreach (var symbol in buffer.GetSymbols())
        {
            var ticks = buffer.GetTicks(symbol);
            if (ticks.Count < 2)
            {
                result[symbol] = Array.Empty<double>();
                continue;
            }
            
            var returns = new double[ticks.Count];
            var firstPrice = ticks[0].MidPrice;
            
            for (int i = 0; i < ticks.Count; i++)
            {
                // Cumulative return as percentage
                returns[i] = (ticks[i].MidPrice / firstPrice - 1.0) * 100.0;
            }
            
            result[symbol] = returns;
        }
        
        return result;
    }
    
    public double CalculateGapToBase(string symbol, string baseSymbol, ITickBuffer buffer)
    {
        var symbolTicks = buffer.GetTicks(symbol);
        var baseTicks = buffer.GetTicks(baseSymbol);
        
        if (symbolTicks.Count < 2 || baseTicks.Count < 2)
            return 0;
        
        // Calculate cumulative returns
        var symbolFirstPrice = symbolTicks[0].MidPrice;
        var symbolLastPrice = symbolTicks[^1].MidPrice;
        var symbolReturn = (symbolLastPrice / symbolFirstPrice - 1.0) * 100.0;
        
        var baseFirstPrice = baseTicks[0].MidPrice;
        var baseLastPrice = baseTicks[^1].MidPrice;
        var baseReturn = (baseLastPrice / baseFirstPrice - 1.0) * 100.0;
        
        return symbolReturn - baseReturn;
    }
    
    public List<GapChartPoint> CalculateGapChartData(string baseSymbol, ITickBuffer buffer)
    {
        var result = new List<GapChartPoint>();
        var symbols = buffer.GetSymbols();
        
        // Get base ticks first
        var baseTicks = buffer.GetTicks(baseSymbol);
        if (baseTicks.Count < 2)
        {
            // If base has no data, just show all symbols at 0
            foreach (var symbol in symbols)
            {
                var ticks = buffer.GetTicks(symbol);
                foreach (var tick in ticks)
                {
                    result.Add(new GapChartPoint
                    {
                        Symbol = symbol,
                        Timestamp = tick.Timestamp,
                        GapToBase = 0
                    });
                }
            }
            return result.OrderBy(p => p.Timestamp).ToList();
        }
        
        // Find common start time - the latest start time among all symbols
        var startTimes = new List<DateTime>();
        foreach (var symbol in symbols)
        {
            var ticks = buffer.GetTicks(symbol);
            if (ticks.Count >= 2)
                startTimes.Add(ticks[0].Timestamp);
        }
        
        if (startTimes.Count == 0)
            return result;
        
        var commonStartTime = startTimes.Max();
        
        // Pre-calculate base returns with common start time
        var baseFirstPrice = GetPriceAtTime(baseTicks, commonStartTime);
        if (baseFirstPrice <= 0)
            baseFirstPrice = baseTicks[0].MidPrice;
        
        foreach (var symbol in symbols)
        {
            var ticks = buffer.GetTicks(symbol);
            if (ticks.Count < 2)
                continue;
            
            // Get first price at common start time for this symbol
            var symbolFirstPrice = GetPriceAtTime(ticks, commonStartTime);
            if (symbolFirstPrice <= 0)
                symbolFirstPrice = ticks[0].MidPrice;
            
            foreach (var tick in ticks)
            {
                // Only process ticks after common start time
                if (tick.Timestamp < commonStartTime)
                    continue;
                
                var symbolReturn = (tick.MidPrice / symbolFirstPrice - 1.0) * 100.0;
                
                double gapToBase;
                if (symbol == baseSymbol)
                {
                    gapToBase = 0;
                }
                else
                {
                    // Find base return at same time
                    var basePrice = GetPriceAtTime(baseTicks, tick.Timestamp);
                    if (basePrice <= 0)
                        basePrice = baseFirstPrice;
                    
                    var baseReturn = (basePrice / baseFirstPrice - 1.0) * 100.0;
                    gapToBase = symbolReturn - baseReturn;
                }
                
                // Sanity check - avoid extreme values
                if (double.IsNaN(gapToBase) || double.IsInfinity(gapToBase))
                    gapToBase = 0;
                
                result.Add(new GapChartPoint
                {
                    Symbol = symbol,
                    Timestamp = tick.Timestamp,
                    GapToBase = gapToBase
                });
            }
        }
        
        return result.OrderBy(p => p.Timestamp).ToList();
    }
    
    private double GetPriceAtTime(IReadOnlyList<TickData> ticks, DateTime timestamp)
    {
        if (ticks.Count == 0)
            return 0;
        
        // Find closest tick at or before the timestamp
        TickData? closestTick = null;
        foreach (var tick in ticks)
        {
            if (tick.Timestamp <= timestamp)
                closestTick = tick;
            else if (closestTick == null)
                closestTick = tick; // Use first tick if all are after timestamp
            else
                break;
        }
        
        return closestTick?.MidPrice ?? 0;
    }
    
    private double[] CalculateReturnSeries(string symbol, ITickBuffer buffer)
    {
        var ticks = buffer.GetTicks(symbol);
        if (ticks.Count < 2)
            return Array.Empty<double>();
        
        var returns = new double[ticks.Count - 1];
        for (int i = 1; i < ticks.Count; i++)
        {
            var prevPrice = ticks[i - 1].MidPrice;
            var currPrice = ticks[i].MidPrice;
            returns[i - 1] = prevPrice != 0 ? (currPrice / prevPrice - 1.0) : 0;
        }
        
        return returns;
    }
    
    public LagCorrelationResult CalculateLagCorrelation(
        string symbol,
        string baseSymbol,
        ITickBuffer buffer,
        int lagRangeSeconds = 5,
        int lagStepMs = 100)
    {
        if (symbol == baseSymbol)
        {
            return new LagCorrelationResult
            {
                Symbol = symbol,
                BestLagSeconds = 0,
                BestCorrelation = 1.0
            };
        }
        
        var symbolTicks = buffer.GetTicks(symbol);
        var baseTicks = buffer.GetTicks(baseSymbol);
        
        if (symbolTicks.Count < 10 || baseTicks.Count < 10)
        {
            return new LagCorrelationResult
            {
                Symbol = symbol,
                BestLagSeconds = 0,
                BestCorrelation = 0
            };
        }
        
        // Calculate returns for both series
        var symbolReturns = CalculateReturnSeries(symbol, buffer);
        var baseReturns = CalculateReturnSeries(baseSymbol, buffer);
        
        if (symbolReturns.Length < 5 || baseReturns.Length < 5)
        {
            return new LagCorrelationResult
            {
                Symbol = symbol,
                BestLagSeconds = 0,
                BestCorrelation = 0
            };
        }
        
        // Create time-aligned series for correlation
        var symbolWithTime = symbolTicks.Skip(1).Zip(symbolReturns, (t, r) => (t.Timestamp, r)).ToList();
        var baseWithTime = baseTicks.Skip(1).Zip(baseReturns, (t, r) => (t.Timestamp, r)).ToList();
        
        double bestCorrelation = 0;
        double bestLagMs = 0;
        
        // Test different lags
        int lagSteps = (lagRangeSeconds * 1000) / lagStepMs;
        
        for (int lagIdx = -lagSteps; lagIdx <= lagSteps; lagIdx++)
        {
            double lagMs = lagIdx * lagStepMs;
            var correlation = CalculateCorrelationAtLag(symbolWithTime, baseWithTime, TimeSpan.FromMilliseconds(lagMs));
            
            if (Math.Abs(correlation) > Math.Abs(bestCorrelation))
            {
                bestCorrelation = correlation;
                bestLagMs = lagMs;
            }
        }
        
        return new LagCorrelationResult
        {
            Symbol = symbol,
            BestLagSeconds = bestLagMs / 1000.0,
            BestCorrelation = bestCorrelation
        };
    }
    
    private double CalculateCorrelationAtLag(
        List<(DateTime Timestamp, double Return)> symbolSeries,
        List<(DateTime Timestamp, double Return)> baseSeries,
        TimeSpan lag)
    {
        // Shift symbol series by lag and find matching pairs
        var pairs = new List<(double symbol, double baseVal)>();
        
        foreach (var symbolPoint in symbolSeries)
        {
            var shiftedTime = symbolPoint.Timestamp.Add(-lag);
            
            // Find closest base point
            var basePoint = baseSeries
                .Where(b => Math.Abs((b.Timestamp - shiftedTime).TotalMilliseconds) < 500)
                .OrderBy(b => Math.Abs((b.Timestamp - shiftedTime).TotalMilliseconds))
                .FirstOrDefault();
            
            if (basePoint != default)
            {
                pairs.Add((symbolPoint.Return, basePoint.Return));
            }
        }
        
        if (pairs.Count < 5)
            return 0;
        
        return CalculatePearsonCorrelation(
            pairs.Select(p => p.symbol).ToArray(),
            pairs.Select(p => p.baseVal).ToArray());
    }
    
    private double CalculatePearsonCorrelation(double[] x, double[] y)
    {
        if (x.Length != y.Length || x.Length < 2)
            return 0;
        
        int n = x.Length;
        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0, sumY2 = 0;
        
        for (int i = 0; i < n; i++)
        {
            sumX += x[i];
            sumY += y[i];
            sumXY += x[i] * y[i];
            sumX2 += x[i] * x[i];
            sumY2 += y[i] * y[i];
        }
        
        double numerator = n * sumXY - sumX * sumY;
        double denominator = Math.Sqrt((n * sumX2 - sumX * sumX) * (n * sumY2 - sumY * sumY));
        
        if (denominator == 0)
            return 0;
        
        return numerator / denominator;
    }
    
    public List<LagCorrelationResult> CalculateAllLagCorrelations(
        string baseSymbol,
        ITickBuffer buffer,
        int lagRangeSeconds = 5,
        int lagStepMs = 100)
    {
        var results = new List<LagCorrelationResult>();
        
        foreach (var symbol in buffer.GetSymbols())
        {
            if (symbol == baseSymbol)
                continue;
            
            var result = CalculateLagCorrelation(symbol, baseSymbol, buffer, lagRangeSeconds, lagStepMs);
            results.Add(result);
        }
        
        return results.OrderByDescending(r => Math.Abs(r.BestCorrelation)).ToList();
    }
}
