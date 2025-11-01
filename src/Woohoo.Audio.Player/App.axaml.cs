// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Woohoo.Audio.Core.CueToolsDatabase;
using Woohoo.Audio.Core.Metadata;
using Woohoo.Audio.Player.Services;
using Woohoo.Audio.Player.ViewModels;
using Woohoo.Audio.Player.Views;

public partial class App : Application
{
    public static TopLevel? GetTopLevel(Application app)
    {
        if (app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }

        if (app.ApplicationLifetime is ISingleViewApplicationLifetime viewApp)
        {
            var visualRoot = viewApp.MainView?.GetVisualRoot();
            return visualRoot as TopLevel;
        }

        return null;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        // Register all the services needed for the application to run
        var collection = new ServiceCollection();
        RegisterServices(collection);

        // Creates a ServiceProvider containing services from the provided IServiceCollection
        var services = collection.BuildServiceProvider();

        var vm = services.GetRequiredService<MainWindowViewModel>();

        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void RegisterServices(ServiceCollection collection)
    {
        collection.AddTransient<MainWindowViewModel>();
        collection.AddTransient<IFilePickerService, FilePickerService>();
        collection.AddTransient<IMetadataProvider, MetadataProvider>();

        if (OperatingSystem.IsWindowsVersionAtLeast(5, 1, 2600))
        {
            collection.AddTransient<IPowerManagementService, WindowsPowerManagementService>();
        }
        else if (OperatingSystem.IsMacOS())
        {
            collection.AddTransient<IPowerManagementService, MacOSPowerManagementService>();
        }
        else
        {
            collection.AddTransient<IPowerManagementService, NullPowerManagementService>();
        }
    }
}
