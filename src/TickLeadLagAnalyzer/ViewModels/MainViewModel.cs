using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using TickLeadLagAnalyzer.Domain.Interfaces;
using TickLeadLagAnalyzer.Domain.Models;

namespace TickLeadLagAnalyzer.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IMt5ConnectionService _connectionService;
    private readonly ITickBuffer _tickBuffer;
    private readonly IAnalysisService _analysisService;
    private readonly ILogger<MainViewModel> _logger;
    private readonly DispatcherTimer _updateTimer;
    private readonly DispatcherTimer _correlationTimer;
    private readonly SynchronizationContext? _syncContext;
    private CancellationTokenSource? _connectionCts;
    
    // Chart series storage
    private readonly Dictionary<string, ObservableCollection<DateTimePoint>> _chartData = new();
    private readonly Dictionary<string, LineSeries<DateTimePoint>> _chartSeries = new();
    
    // Colors for different symbols
    private static readonly SKColor[] SymbolColors =
    [
        SKColors.LimeGreen,
        SKColors.Orange,
        SKColors.DeepSkyBlue,
        SKColors.Gold,
        SKColors.OrangeRed,
        SKColors.Magenta,
        SKColors.Cyan,
        SKColors.Yellow,
        SKColors.Pink,
        SKColors.LightGreen
    ];
    
    [ObservableProperty]
    private string _server = "185.97.160.70:443";
    
    [ObservableProperty]
    private string _login = "5002166";
    
    [ObservableProperty]
    private string _password = "!l2kCkPka";
    
    [ObservableProperty]
    private string _connectionStatusText = "Disconnected";
    
    [ObservableProperty]
    private string _lastTickTime = "--:--:--";
    
    [ObservableProperty]
    private bool _isConnected;
    
    [ObservableProperty]
    private bool _isConnecting;
    
    [ObservableProperty]
    private string _newSymbol = string.Empty;
    
    [ObservableProperty]
    private SymbolViewModel? _selectedSymbol;
    
    [ObservableProperty]
    private string? _baseSymbol;
    
    [ObservableProperty]
    private int _windowDuration = 60;
    
    [ObservableProperty]
    private int _maxTicks = 2000;
    
    [ObservableProperty]
    private int _lagRange = 5;
    
    public ObservableCollection<SymbolViewModel> Symbols { get; } = new();
    
    public ObservableCollection<LagCorrelationViewModel> CorrelationResults { get; } = new();
    
    public ObservableCollection<ISeries> ChartSeries { get; } = new();
    
    public Axis[] XAxes { get; }
    
    public Axis[] YAxes { get; }
    
    public MainViewModel(
        IMt5ConnectionService connectionService,
        ITickBuffer tickBuffer,
        IAnalysisService analysisService,
        ILogger<MainViewModel> logger)
    {
        _connectionService = connectionService;
        _tickBuffer = tickBuffer;
        _analysisService = analysisService;
        _logger = logger;
        _syncContext = SynchronizationContext.Current;
        
        // Initialize axes
        XAxes =
        [
            new DateTimeAxis(TimeSpan.FromSeconds(1), date => date.ToString("HH:mm:ss"))
            {
                Name = "Time",
                NamePaint = new SolidColorPaint(SKColors.White),
                LabelsPaint = new SolidColorPaint(SKColors.LightGray),
                SeparatorsPaint = new SolidColorPaint(SKColors.DimGray) { StrokeThickness = 1 },
                AnimationsSpeed = TimeSpan.FromMilliseconds(0),
            }
        ];
        
        YAxes =
        [
            new Axis
            {
                Name = "Gap to Base (%)",
                NamePaint = new SolidColorPaint(SKColors.White),
                LabelsPaint = new SolidColorPaint(SKColors.LightGray),
                SeparatorsPaint = new SolidColorPaint(SKColors.DimGray) { StrokeThickness = 1 },
                Labeler = value => $"{value:+0.00%;-0.00%;0.00%}",
                MinLimit = -0.01,  // Minimum -1%
                MaxLimit = 0.01,   // Maximum +1%
                AnimationsSpeed = TimeSpan.FromMilliseconds(0),
            }
        ];
        
        // Subscribe to events
        _connectionService.StatusChanged += OnConnectionStatusChanged;
        _connectionService.TickReceived += OnTickReceived;
        
        // Setup update timer for chart (throttled)
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _updateTimer.Tick += OnUpdateTimerTick;
        
        // Setup correlation timer (less frequent)
        _correlationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _correlationTimer.Tick += OnCorrelationTimerTick;
        
        // Add default symbols
        AddDefaultSymbols();
        
        _logger.LogInformation("MainViewModel initialized");
    }
    
    private void AddDefaultSymbols()
    {
        var defaultSymbols = new[] { "EURUSD", "GBPUSD", "XAUUSD", "USDJPY", "USOIL" };
        foreach (var symbol in defaultSymbols)
        {
            var vm = new SymbolViewModel { Symbol = symbol };
            Symbols.Add(vm);
        }
        
        if (Symbols.Count > 0)
        {
            BaseSymbol = Symbols[0].Symbol;
            Symbols[0].IsBase = true;
        }
    }
    
    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (IsConnected || IsConnecting)
            return;
        
        IsConnecting = true;
        _connectionCts = new CancellationTokenSource();
        
        try
        {
            if (!ulong.TryParse(Login, out var loginNumber))
            {
                _logger.LogWarning("Invalid login number");
                return;
            }
            
            var success = await _connectionService.ConnectAsync(Server, loginNumber, Password, _connectionCts.Token);
            
            if (success)
            {
                // Update available symbols
                var availableSymbols = _connectionService.GetAvailableSymbols();
                _logger.LogInformation("Connected. Available symbols: {Count}", availableSymbols.Count);
                
                // Clear default symbols that don't exist
                var symbolsToRemove = Symbols.Where(s => !_connectionService.SymbolExists(s.Symbol)).ToList();
                foreach (var s in symbolsToRemove)
                {
                    Symbols.Remove(s);
                }
                
                // Add some default symbols that exist on the server
                if (Symbols.Count == 0)
                {
                    var commonSymbols = new[] { "EURUSD", "GBPUSD", "XAUUSD", "USDJPY", "USOIL", "BTCUSD" };
                    foreach (var sym in commonSymbols)
                    {
                        // Try to find on the server with various suffixes
                        var found = availableSymbols.FirstOrDefault(s => 
                            s.StartsWith(sym, StringComparison.OrdinalIgnoreCase));
                        if (found != null && !Symbols.Any(x => x.Symbol == found))
                        {
                            Symbols.Add(new SymbolViewModel { Symbol = found });
                            if (Symbols.Count >= 5) break;
                        }
                    }
                    
                    if (Symbols.Count > 0)
                    {
                        BaseSymbol = Symbols[0].Symbol;
                        Symbols[0].IsBase = true;
                    }
                }
                
                // Subscribe to symbols
                foreach (var symbol in Symbols)
                {
                    await _connectionService.SubscribeAsync(symbol.Symbol);
                }
                
                _updateTimer.Start();
                _correlationTimer.Start();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting");
        }
        finally
        {
            IsConnecting = false;
        }
    }
    
    [RelayCommand]
    private async Task DisconnectAsync()
    {
        if (!IsConnected)
            return;
        
        _connectionCts?.Cancel();
        _updateTimer.Stop();
        _correlationTimer.Stop();
        
        await _connectionService.DisconnectAsync();
        
        _tickBuffer.Clear();
        ChartSeries.Clear();
        _chartData.Clear();
        _chartSeries.Clear();
        CorrelationResults.Clear();
    }
    
    [RelayCommand]
    private async Task AddSymbolAsync()
    {
        if (string.IsNullOrWhiteSpace(NewSymbol))
            return;
        
        var symbol = NewSymbol.ToUpperInvariant().Trim();
        
        if (Symbols.Any(s => s.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("Symbol {Symbol} already exists", symbol);
            return;
        }
        
        var vm = new SymbolViewModel { Symbol = symbol };
        Symbols.Add(vm);
        
        if (IsConnected)
        {
            await _connectionService.SubscribeAsync(symbol);
        }
        
        NewSymbol = string.Empty;
        _logger.LogInformation("Added symbol {Symbol}", symbol);
    }
    
    [RelayCommand]
    private async Task RemoveSymbolAsync()
    {
        if (SelectedSymbol == null)
            return;
        
        var symbol = SelectedSymbol.Symbol;
        
        if (IsConnected)
        {
            await _connectionService.UnsubscribeAsync(symbol);
        }
        
        _tickBuffer.ClearSymbol(symbol);
        
        // Remove from chart
        if (_chartSeries.TryGetValue(symbol, out var series))
        {
            ChartSeries.Remove(series);
            _chartSeries.Remove(symbol);
            _chartData.Remove(symbol);
        }
        
        // Update base if needed
        if (BaseSymbol == symbol && Symbols.Count > 1)
        {
            var newBase = Symbols.FirstOrDefault(s => s.Symbol != symbol);
            if (newBase != null)
            {
                SetBaseSymbol(newBase.Symbol);
            }
        }
        
        Symbols.Remove(SelectedSymbol);
        SelectedSymbol = null;
        
        _logger.LogInformation("Removed symbol {Symbol}", symbol);
    }
    
    [RelayCommand]
    private void SetAsBase()
    {
        if (SelectedSymbol == null)
            return;
        
        SetBaseSymbol(SelectedSymbol.Symbol);
    }
    
    private void SetBaseSymbol(string symbol)
    {
        foreach (var s in Symbols)
        {
            s.IsBase = s.Symbol == symbol;
        }
        BaseSymbol = symbol;
        
        // Clear chart data to force recalculation with new base
        foreach (var data in _chartData.Values)
        {
            data.Clear();
        }
        
        _logger.LogInformation("Set base symbol to {Symbol}", symbol);
    }
    
    partial void OnWindowDurationChanged(int value)
    {
        _tickBuffer.UpdateConfiguration(new BufferConfiguration
        {
            WindowDurationSeconds = value,
            MaxTicksPerSymbol = MaxTicks,
            LagRangeSeconds = LagRange
        });
    }
    
    partial void OnMaxTicksChanged(int value)
    {
        _tickBuffer.UpdateConfiguration(new BufferConfiguration
        {
            WindowDurationSeconds = WindowDuration,
            MaxTicksPerSymbol = value,
            LagRangeSeconds = LagRange
        });
    }
    
    private void OnConnectionStatusChanged(object? sender, ConnectionStatus status)
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            ConnectionStatusText = status switch
            {
                ConnectionStatus.Connected => "Connected",
                ConnectionStatus.Connecting => "Connecting...",
                ConnectionStatus.Disconnecting => "Disconnecting...",
                ConnectionStatus.Error => "Error",
                _ => "Disconnected"
            };
            
            IsConnected = status == ConnectionStatus.Connected;
        });
    }
    
    private void OnTickReceived(object? sender, TickData tick)
    {
        // Add to buffer (off UI thread)
        _tickBuffer.AddTick(tick);
        
        // Update symbol state on UI thread
        Application.Current?.Dispatcher.BeginInvoke(() =>
        {
            var symbolVm = Symbols.FirstOrDefault(s => s.Symbol == tick.Symbol);
            if (symbolVm != null)
            {
                symbolVm.Bid = tick.Bid;
                symbolVm.Ask = tick.Ask;
                symbolVm.Spread = tick.Spread;
                symbolVm.LastTickTime = tick.Timestamp;
            }
            
            LastTickTime = tick.Timestamp.ToLocalTime().ToString("HH:mm:ss");
        });
    }
    
    private void OnUpdateTimerTick(object? sender, EventArgs e)
    {
        UpdateChart();
    }
    
    private void OnCorrelationTimerTick(object? sender, EventArgs e)
    {
        UpdateCorrelations();
    }
    
    private void UpdateChart()
    {
        if (string.IsNullOrEmpty(BaseSymbol))
            return;
        
        try
        {
            var chartPoints = _analysisService.CalculateGapChartData(BaseSymbol, _tickBuffer);
            var groupedBySymbol = chartPoints.GroupBy(p => p.Symbol);
            
            double minValue = double.MaxValue;
            double maxValue = double.MinValue;
            DateTime minTime = DateTime.MaxValue;
            DateTime maxTime = DateTime.MinValue;
            
            int colorIndex = 0;
            foreach (var group in groupedBySymbol)
            {
                var symbol = group.Key;
                var points = group.ToList();
                
                if (!_chartData.ContainsKey(symbol))
                {
                    _chartData[symbol] = new ObservableCollection<DateTimePoint>();
                    var color = SymbolColors[colorIndex % SymbolColors.Length];
                    
                    var series = new LineSeries<DateTimePoint>
                    {
                        Values = _chartData[symbol],
                        Name = symbol,
                        Stroke = new SolidColorPaint(color) { StrokeThickness = 2 },
                        Fill = null,
                        GeometryFill = null,
                        GeometryStroke = null,
                        LineSmoothness = 0
                    };
                    
                    _chartSeries[symbol] = series;
                    ChartSeries.Add(series);
                }
                
                var data = _chartData[symbol];
                data.Clear();
                
                // Add only last N points for performance
                var recentPoints = points.TakeLast(500);
                foreach (var point in recentPoints)
                {
                    var yValue = point.GapToBase / 100.0; // Convert to decimal
                    data.Add(new DateTimePoint(point.Timestamp, yValue));
                    
                    // Track min/max for axis limits
                    if (yValue < minValue) minValue = yValue;
                    if (yValue > maxValue) maxValue = yValue;
                    if (point.Timestamp < minTime) minTime = point.Timestamp;
                    if (point.Timestamp > maxTime) maxTime = point.Timestamp;
                }
                
                colorIndex++;
            }
            
            // Update X axis to auto-scroll (show last 60 seconds window)
            if (maxTime > DateTime.MinValue)
            {
                var windowStart = maxTime.AddSeconds(-60);
                if (windowStart < minTime) windowStart = minTime;
                
                XAxes[0].MinLimit = windowStart.Ticks;
                XAxes[0].MaxLimit = maxTime.Ticks;
            }
            
            // Update Y axis with padding but ensure minimum range
            if (minValue < double.MaxValue && maxValue > double.MinValue)
            {
                var range = maxValue - minValue;
                var minRange = 0.0002; // Minimum range of 0.02%
                
                if (range < minRange)
                {
                    var center = (minValue + maxValue) / 2;
                    minValue = center - minRange / 2;
                    maxValue = center + minRange / 2;
                }
                else
                {
                    // Add 10% padding
                    var padding = range * 0.1;
                    minValue -= padding;
                    maxValue += padding;
                }
                
                YAxes[0].MinLimit = minValue;
                YAxes[0].MaxLimit = maxValue;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating chart");
        }
    }
    
    private void UpdateCorrelations()
    {
        if (string.IsNullOrEmpty(BaseSymbol))
            return;
        
        try
        {
            var results = _analysisService.CalculateAllLagCorrelations(BaseSymbol, _tickBuffer, LagRange, 100);
            
            CorrelationResults.Clear();
            foreach (var result in results)
            {
                CorrelationResults.Add(new LagCorrelationViewModel
                {
                    Symbol = result.Symbol,
                    BestLag = result.BestLagSeconds,
                    Correlation = result.BestCorrelation,
                    IsLeading = result.IsLeading
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating correlations");
        }
    }
    
    public void Dispose()
    {
        _updateTimer.Stop();
        _correlationTimer.Stop();
        _connectionService.StatusChanged -= OnConnectionStatusChanged;
        _connectionService.TickReceived -= OnTickReceived;
        try
        {
            _connectionCts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Already disposed, ignore
        }
        _connectionCts?.Dispose();
    }
}

public partial class SymbolViewModel : ObservableObject
{
    [ObservableProperty]
    private string _symbol = string.Empty;
    
    [ObservableProperty]
    private double _bid;
    
    [ObservableProperty]
    private double _ask;
    
    [ObservableProperty]
    private double _spread;
    
    [ObservableProperty]
    private DateTime _lastTickTime;
    
    [ObservableProperty]
    private bool _isBase;
}

public partial class LagCorrelationViewModel : ObservableObject
{
    [ObservableProperty]
    private string _symbol = string.Empty;
    
    [ObservableProperty]
    private double _bestLag;
    
    [ObservableProperty]
    private double _correlation;
    
    [ObservableProperty]
    private bool _isLeading;
    
    public string LagDisplay => $"{BestLag:+0.0;-0.0;0.0} s";
    public string CorrelationDisplay => $"{Correlation:0.00}";
}
