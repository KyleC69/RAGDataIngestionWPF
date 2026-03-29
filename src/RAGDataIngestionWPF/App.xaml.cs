// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         App.xaml.cs
// Author: Kyle L. Crowder
// Build Num: 073042



#nullable enable

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

using DataIngestionLib.Agents;
using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Data;
using DataIngestionLib.DocIngestion;
using DataIngestionLib.Providers;
using DataIngestionLib.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Notifications;

using RAGDataIngestionWPF.Activation;
using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Contracts.Settings;
using RAGDataIngestionWPF.Core.Services;
using RAGDataIngestionWPF.Services;
using RAGDataIngestionWPF.ViewModels;
using RAGDataIngestionWPF.Views;

using SystemConfigurationManager = System.Configuration.ConfigurationManager;




namespace RAGDataIngestionWPF;





public sealed partial class App : Application
{
    private IAppCancellationTokenProvider? _cancellationProvider;
    private IHost? _host;
    private bool _isHostStarted;

    private LogLevel _loglevel;

    public static IHost AppHost { get; private set; } = null!;

    internal new static App Current
    {
        get { return (App)Application.Current; }
    }








    private IHost BuildHost()
    {
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error | SourceLevels.Warning;
        PresentationTraceSources.ResourceDictionarySource.Switch.Level = SourceLevels.All;
        PresentationTraceSources.AnimationSource.Switch.Level = SourceLevels.All;

        _loglevel = SystemConfigurationManager.AppSettings["MinimumLogLevel"] != null && Enum.TryParse(SystemConfigurationManager.AppSettings["MinimumLogLevel"], true, out LogLevel configLevel) ? configLevel : LogLevel.Trace;
        return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(c =>
                {

                    IConfigurationBuilder unused4 = c.SetBasePath(Environment.CurrentDirectory);
                })
                .ConfigureServices(ConfigureServices)
                .ConfigureLogging(logging =>
                {
                    ILoggingBuilder unused3 = logging.AddDebug();
                    ILoggingBuilder unused2 = logging.AddConsole();
                    // Set the host-level minimum to Trace so every message reaches
                    // the dynamic filter below. The LoggingLevelSwitch controls the
                    // effective minimum at runtime and is user-configurable from the
                    // Settings page.
                    logging.AddJsonConsole(options => { options.JsonWriterOptions = new System.Text.Json.JsonWriterOptions { Indented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }; });
                    ILoggingBuilder unused1 = logging.SetMinimumLevel(_loglevel);
                    ILoggingBuilder unused = logging.AddFilter((_, level) => level >= _loglevel);
                    logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);




                })
                .Build();
    }








    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(services);

        RegisterHostServices(services);
        RegisterAgentServices(services);
        RegisterActivationHandlers(services);
        RegisterCoreServices(services);
        RegisterApplicationServices(services);
        RegisterViewsAndViewModels(services);

    }








    private async Task EnsureHostStartedAsync()
    {
        if (_host is null || _isHostStarted)
        {
            return;
        }


        try
        {
            if (_isHostStarted)
            {
                return;
            }

            await _host.StartAsync();
            _isHostStarted = true;
        }
        catch (Exception ex)
        {
            var logger = _host?.Services.GetService<ILogger<App>>();
            if (logger != null)
            {
                LogUnhandledUiException(logger, ex);
            }
        }
    }








    private static string GetAppLocation()
    {
        var entryAssemblyLocation = Assembly.GetEntryAssembly()?.Location;
        var appLocation = string.IsNullOrWhiteSpace(entryAssemblyLocation) ? AppContext.BaseDirectory : Path.GetDirectoryName(entryAssemblyLocation);

        return string.IsNullOrWhiteSpace(appLocation) ? AppContext.BaseDirectory : appLocation;
    }








    private async Task HandleToastActivationAsync(string toastArgument)
    {
        if (_host is null)
        {
            return;
        }

        IConfiguration configuration = _host.Services.GetRequiredService<IConfiguration>();
        configuration[ToastNotificationActivationHandler.ACTIVATION_ARGUMENTS] = toastArgument;
        await EnsureHostStartedAsync();
    }








    [LoggerMessage(LogLevel.Error, "Unhandled UI exception.")]
    static partial void LogUnhandledUiException(ILogger<App> logger, Exception exception);








    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var logger = _host?.Services.GetService<ILogger<App>>();
        if (logger != null)
        {
            LogUnhandledUiException(logger, e.Exception);
        }

        // Cancel all running operations on app crash
        _cancellationProvider?.CancelAll();

        e.Handled = false;
    }








    private async void OnExit(object sender, ExitEventArgs e)
    {
        if (_host is null)
        {
            return;
        }

        var logger = _host.Services.GetService<ILogger<App>>();

        try
        {
            // Signal cancellation to all running operations on normal exit
            _cancellationProvider?.CancelAll();

            if (_isHostStarted)
            {
                await _host.StopAsync();
            }
        }
        catch (InvalidOperationException ex)
        {
            if (logger != null)
            {
                LogUnhandledUiException(logger, ex);
            }
        }
        catch (OperationCanceledException ex)
        {
            Debug.Assert(logger != null, nameof(logger) + " != null");
            LogUnhandledUiException(logger, ex);
        }
        finally
        {
            // Dispose the cancellation provider
            if (_cancellationProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _host.Dispose();
            _host = null;
            _isHostStarted = false;
        }
    }








    private async void OnStartup(object sender, StartupEventArgs e)
    {
        // https://docs.microsoft.com/windows/apps/design/shell/tiles-and-notifications/send-local-toast?tabs=desktop
        ToastNotificationManagerCompat.OnActivated += toastArgs =>
        {
            Task unused = Current.Dispatcher.Invoke(() => _ = HandleToastActivationAsync(toastArgs.Argument));
        };
        // For more information about .NET generic host see  https://docs.microsoft.com/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.0

        _host = BuildHost();
        AppHost = _host;

        // Initialize app-wide cancellation provider from the DI container
        _cancellationProvider = _host.Services.GetRequiredService<IAppCancellationTokenProvider>();

        if (ToastNotificationManagerCompat.WasCurrentProcessToastActivated())
        {
            // ToastNotificationActivator code will run after this completes and will show a window if necessary.
            return;
        }

        await EnsureHostStartedAsync();
    }








    private static void RegisterActivationHandlers(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _ = services.AddSingleton<ToastNotificationActivationHandler>();
    }








    private static void RegisterAgentServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);


        services.AddSingleton<SqlChatHistoryProvider>();
        IServiceCollection unused4 = services.AddSingleton<IChatHistoryProvider>(provider => provider.GetRequiredService<SqlChatHistoryProvider>());
        IServiceCollection unused5 = services.AddSingleton<ISQLChatHistoryProvider>(provider => provider.GetRequiredService<SqlChatHistoryProvider>());
        _ = services.AddSingleton<RagDataService>();
        _ = services.AddSingleton<ChunkMetadataGenerator>();
        _ = services.AddSingleton<DocIngestionPipeline>();
        _ = services.AddSingleton<SqlTableMaint>();
        _ = services.AddSingleton<LocalRagContextSource>();
        IServiceCollection unused2 = services.AddSingleton<AgentFactory>();

        IServiceCollection unused1 = services.AddSingleton<ChatHistoryContextInjector>();
        _ = services.AddSingleton<AIContextRAGInjector>();
    }








    private static void RegisterApplicationServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _ = services.AddSingleton<AppCancellationTokenProvider>();
        _ = services.AddSingleton<ToastNotificationsService>();
        _ = services.AddSingleton<ApplicationInfoService>();
        _ = services.AddSingleton<PersistAndRestoreService>();
        _ = services.AddSingleton<SystemService>();
        _ = services.AddSingleton<AppSettings>();
        _ = services.AddSingleton<ConversationSessionBootstrapper>();
        _ = services.AddSingleton<ConversationHistoryLoader>();
        _ = services.AddSingleton<ConversationTokenCounter>();
        _ = services.AddSingleton<PageService>();
        _ = services.AddSingleton<NavigationService>();
        _ = services.AddSingleton<UserDataService>();
    }








    private static void RegisterCoreServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _ = services.AddSingleton<IdentityService>();
        _ = services.AddSingleton<FileService>();
    }








    private static void RegisterHostServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _ = services.AddHostedService<ApplicationHostService>();
        services.AddDbContext<AIChatHistoryDb>();
        services.AddDbContext<RAGContext>();
    }








    private static void RegisterViewsAndViewModels(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _ = services.AddTransient<ShellWindow>();
        _ = services.AddTransient<ShellViewModel>();
        _ = services.AddTransient<MainViewModel>();
        _ = services.AddTransient<MainPage>();
        _ = services.AddTransient<DataGridViewModel>();
        _ = services.AddTransient<DataGridPage>();
        _ = services.AddTransient<WebViewViewModel>();
        _ = services.AddTransient<WebViewPage>();
        _ = services.AddTransient<SettingsViewModel>();
        _ = services.AddTransient<SettingsPage>();
        _ = services.AddTransient<LogInWindow>();
        _ = services.AddTransient<LogInViewModel>();

    }
}