// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue;

using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Serilog;
using Windows.Storage;
using WinUIEx;
using Woohoo.Audio.Core;
using Woohoo.Audio.Core.Playback;
using Woohoo.Audio.Playback.Sdl3;
using Woohoo.Audio.Playback.Windows;
using Woohoo.Audio.Services;
using Woohoo.Audio.Services.Impl;
using Woohoo.Discue.Contracts.Services;
using Woohoo.Discue.Services;
using Woohoo.Discue.ViewModels;
using Woohoo.Discue.Views;

public partial class App : Application
{
    private WindowEx? window;

    public App()
    {
        this.InitializeComponent();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(GetLogFilePath(), flushToDiskInterval: TimeSpan.FromSeconds(2))
            .CreateLogger();

        this.Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .UseSerilog()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IAudioPlayerProvider>(serviceProvider =>
                {
                    var factoryMap = new Dictionary<string, Func<IAudioPlayer>>
                    {
                        { AudioEngineType.WindowsMediaPlayer.ToString(), () => new WindowsAudioPlayer() },
                        { AudioEngineType.Sdl3.ToString(), () => new Sdl3AudioPlayer() },
                    };

                    return new AudioPlayerProvider(
                        serviceProvider.GetRequiredService<ILocalSettingsService>(),
                        AudioEngineType.WindowsMediaPlayer.ToString(),
                        factoryMap);
                });

                services.AddSingleton<IBitmapCacheService>(serviceProvider =>
                {
                    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

                    return new BitmapCacheService(httpClientFactory)
                    {
                        CacheFolderPath = ApplicationData.Current.LocalCacheFolder.Path,
                    };
                });
                services.AddSingleton<IWindowsBitmapCacheService, WindowsBitmapCacheService>();
                services.AddSingleton<IDispatcherQueueService, DispatcherQueueService>();
                services.AddSingleton<IHttpClientFactory, HttpClientFactory>();
                services.AddSingleton<ILocalSettingsService>(_ =>
                {
                    var settingsFilePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Woohoo.Discue.WinUI",
                        "ApplicationData",
                        "LocalSettings.json");
                    return new LocalSettingsService() { FilePath = settingsFilePath };
                });
                services.AddSingleton<IMediaPlayerService, MediaPlayerService>();
                services.AddSingleton<IMruService>(_ =>
                {
                    var mruFilePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Woohoo.Discue.WinUI",
                        "ApplicationData",
                        "Mru.json");
                    return new MruService() { MruFilePath = mruFilePath };
                });
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddTransient<INavigationViewService, NavigationViewService>();
                services.AddSingleton<IPageService, PageService>();
                services.AddSingleton<IRestartService, RestartService>();
                services.AddSingleton<IVisualizationProviderService, VisualizationProviderService>();

                // Windows
                services.AddSingleton<MainWindow>();

                // Pages
                services.AddSingleton<HomePage>();
                services.AddSingleton<HomeViewModel>();
                services.AddSingleton<LyricsPage>();
                services.AddSingleton<LyricsViewModel>();
                services.AddSingleton<NowPlayingPage>();
                services.AddSingleton<NowPlayingViewModel>();
                services.AddSingleton<PlaylistPage>();
                services.AddSingleton<PlaylistViewModel>();
                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<VisualizationPage>();
                services.AddSingleton<VisualizationViewModel>();

                // User Controls
                services.AddSingleton<PlaybackViewModel>();
                services.AddSingleton<ShellViewModel>();
            })
            .Build();
    }

    public static WindowEx? MainWindow { get; set; }

    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    protected static string GetLogFilePath()
    {
        var logsFolder = Path.GetTempPath();
        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);
        return Path.Combine(logsFolder, $"Woohoo.Discue.WinUI-{stamp}.log");
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        this.window = GetService<MainWindow>();
        this.window.Activate();

        MainWindow = this.window;
    }
}
