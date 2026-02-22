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
        App.Current!.Styles[0] = this.SelectedTheme switch
        {
            KnownThemes.ModernContrast => new ModernContrastTheme(),
            KnownThemes.TurboVision => new TurboVisionTheme(),
            KnownThemes.TurboVisionCompatible => new TurboVisionCompatibleTheme(),
            KnownThemes.TurboVisionGray => new TurboVisionGrayTheme(),
            KnownThemes.TurboVisionElegant => new TurboVisionElegantTheme(),
            _ => new ModernTheme(),
        };

        App.Current!.RequestedThemeVariant = this.SelectedTheme switch
        {
            KnownThemes.ModernDark => ThemeVariant.Dark,
            KnownThemes.ModernLight => ThemeVariant.Light,
            _ => ThemeVariant.Default,
        };

        if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            var oldContext = desktopLifetime.MainWindow!.DataContext;
            desktopLifetime.MainWindow.Content = new MainControl() { DataContext = oldContext };
        }
    }
}
