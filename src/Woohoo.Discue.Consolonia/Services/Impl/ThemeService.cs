// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Consolonia.Services.Impl;

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using global::Consolonia.Themes;
using Woohoo.Discue.Consolonia.Views;

internal class ThemeService : IThemeService
{
    public string SelectedTheme
    {
        get;
        set
        {
            field = value;
            this.ApplyTheme();
        }
    }

    = KnownThemes.ModernDark;

    private void ApplyTheme()
    {
        var mainWindow = (App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (mainWindow is null)
        {
            return;
        }

        // Otherwise Avalonia sets some trash template to WindowsPanel
        mainWindow.Content = null;

        App.Current!.Styles[0] = new ModernTheme();
        App.Current!.RequestedThemeVariant = this.SelectedTheme switch
        {
            KnownThemes.ModernDark => ThemeVariant.Dark,
            KnownThemes.ModernLight => ThemeVariant.Light,
            _ => ThemeVariant.Default,
        };

        mainWindow.Content = new MainControl() { DataContext = mainWindow.DataContext };
    }
}
