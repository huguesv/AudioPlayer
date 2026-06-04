// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Tui.Services;

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Consolonia.Themes;
using Woohoo.Audio.Player.Tui.Views;

internal class ThemeService : IThemeService
{
    public string SelectedTheme
    {
        get => field;
        set
        {
            field = value;
            this.ApplyTheme();
        }
    } = KnownThemes.ModernDark;

    private void ApplyTheme()
    {
        var mainWindow = (App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (mainWindow is null)
        {
            return;
        }

        // Otherwise Avalonia sets some trash template to WindowsPanel
        mainWindow.Content = null;

        App.Current!.Styles[0] = this.SelectedTheme switch
        {
            KnownThemes.ModernContrast => new ModernContrastTheme(),
            KnownThemes.TurboVision => new TurboVisionTheme(),
            KnownThemes.TurboVisionCompatible => new TurboVisionCompatibleTheme(),
            KnownThemes.TurboVisionGray => new TurboVisionGrayTheme(),
            KnownThemes.TurboVisionElegant => new TurboVisionElegantTheme(),
            _ => new ModernTheme(),
        };

        if (App.Current!.Styles[0] is ModernTheme)
        {
            App.Current!.RequestedThemeVariant = this.SelectedTheme switch
            {
                KnownThemes.ModernDark => ThemeVariant.Dark,
                KnownThemes.ModernLight => ThemeVariant.Light,
                _ => ThemeVariant.Default,
            };

        }

        mainWindow.Content = new MainControl() { DataContext = mainWindow.DataContext };
    }
}
