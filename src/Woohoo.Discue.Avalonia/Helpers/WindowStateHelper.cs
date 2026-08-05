// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Avalonia.Helpers;

using System;
using System.IO;
using System.Text.Json;
using global::Avalonia;
using global::Avalonia.Controls;

internal static class WindowStateHelper
{
    public static void TrackWindow(Window window, string settingsFilePath)
    {
        window.Opened += (s, e) => RestoreWindowState(window, settingsFilePath);
        window.Closing += (s, e) => SaveWindowState(window, settingsFilePath);
    }

    private static void SaveWindowState(Window window, string settingsFilePath)
    {
        var settings = new WindowSettings
        {
            X = window.Position.X,
            Y = window.Position.Y,
            Width = !double.IsNaN(window.Width) ? window.Width : window.Bounds.Width,
            Height = !double.IsNaN(window.Height) ? window.Height : window.Bounds.Height,
            WindowState = window.WindowState == WindowState.Minimized
                ? WindowState.Normal
                : window.WindowState,
        };

        try
        {
            var dir = Path.GetDirectoryName(settingsFilePath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsFilePath, json);
        }
        catch (Exception)
        {
            // Log or handle file access exception
        }
    }

    private static void RestoreWindowState(Window window, string settingsFilePath)
    {
        if (!File.Exists(settingsFilePath))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(settingsFilePath);
            var settings = JsonSerializer.Deserialize<WindowSettings>(json);
            if (settings == null)
            {
                return;
            }

            // Restore position and size
            window.Position = new PixelPoint((int)settings.X, (int)settings.Y);
            window.Width = Math.Max(200, settings.Width);
            window.Height = Math.Max(200, settings.Height);

            // Safety check: ensure window is on an active monitor screen
            EnsureVisibleOnScreen(window);

            // Restore state (Maximized, Normal)
            window.WindowState = settings.WindowState;
        }
        catch (Exception)
        {
            // Log or handle read failure
        }
    }

    private static void EnsureVisibleOnScreen(Window window)
    {
        var screens = window.Screens;
        if (screens == null)
        {
            return;
        }

        // Construct the pixel rectangle for the target window position
        var windowRect = new PixelRect(window.Position, new PixelSize((int)window.Width, (int)window.Height));

        // Avalonia provides ScreenFromBounds or ScreenFromPoint
        var targetScreen = screens.ScreenFromBounds(windowRect)
                           ?? screens.ScreenFromPoint(window.Position);

        // If targetScreen is null or window is off-screen, reset to primary monitor
        if (targetScreen == null)
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
}
