// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Tui;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Woohoo.Audio.Core;
using Woohoo.Audio.Core.Lyrics;
using Woohoo.Audio.Core.Metadata;
using Woohoo.Audio.Player.Tui.Services;
using Woohoo.Audio.Player.Tui.ViewModels;
using Woohoo.Audio.Player.Tui.Views;

public partial class App : Application
{
    public static TopLevel? GetTopLevel(Application app)
    {
        if (app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            return desktopLifetime.MainWindow;
        }

        return null;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Register all the services needed for the application to run
        var collection = new ServiceCollection();
        RegisterServices(collection);

        // Creates a ServiceProvider containing services from the provided IServiceCollection
        var services = collection.BuildServiceProvider();

        var vm = services.GetRequiredService<MainViewModel>();

        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.MainWindow = new MainWindow
            {
                DataContext = vm,
            };

            var themeService = services.GetRequiredService<IThemeService>();
            themeService.SelectedTheme = vm.SelectedTheme;
        }


        base.OnFrameworkInitializationCompleted();
    }

    private static void RegisterServices(ServiceCollection collection)
    {
        var lrcLibDatabaseFilePath = Environment.GetEnvironmentVariable("LRCLIB_DB_PATH");
        var lyricsOptions = new LyricsProviderOptions
        {
            UseWeb = true,
            UseWebExternalSources = true,
            UseDatabase = File.Exists(lrcLibDatabaseFilePath),
            DatabaseFilePath = lrcLibDatabaseFilePath,
        };

        collection.AddTransient<MainViewModel>();
        collection.AddTransient<ITopLevelProvider, TopLevelProvider>();
        collection.AddTransient<IThemeService, ThemeService>();
        collection.AddTransient<IFilePickerService, FilePickerService>();
        collection.AddTransient<IMetadataProvider, MetadataProvider>();
        collection.AddTransient<IHttpClientFactory, HttpClientFactory>();
        collection.AddTransient<ILyricsProvider>(sp => new LyricsProvider(lyricsOptions, new HttpClientFactory()));
    }
}
