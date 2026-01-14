using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using TickLeadLagAnalyzer.Domain.Interfaces;
using TickLeadLagAnalyzer.Infrastructure.Services;
using TickLeadLagAnalyzer.ViewModels;
using TickLeadLagAnalyzer.Views;

namespace TickLeadLagAnalyzer;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Handle unhandled exceptions
        DispatcherUnhandledException += (s, args) =>
        {
            Log.Error(args.Exception, "Unhandled exception");
            MessageBox.Show($"Error: {args.Exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };
        
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            Log.Error(args.ExceptionObject as Exception, "Domain unhandled exception");
        };
        
        try
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();
            
            Log.Information("Application starting...");
            
            // Configure services
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
            
            // Show main window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            
            Log.Information("Application started successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start application");
            MessageBox.Show($"Failed to start: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }
    
    private void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });
        
        // Domain services
        services.AddSingleton<ITickBuffer, TickBuffer>();
        services.AddSingleton<IAnalysisService, AnalysisService>();
        services.AddSingleton<IMt5ConnectionService, Mt5ConnectionService>();
        
        // ViewModels
        services.AddSingleton<MainViewModel>();
        
        // Views
        services.AddSingleton<MainWindow>();
    }
    
    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            var vm = _serviceProvider?.GetService<MainViewModel>();
            vm?.Dispose();
        }
        catch { }
        
        Log.CloseAndFlush();
        
        try
        {
            _serviceProvider?.Dispose();
        }
        catch { }
        
        base.OnExit(e);
    }
}
