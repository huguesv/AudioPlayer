// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Tui;

using Avalonia;
using Consolonia;
using Consolonia.Fonts;
using Consolonia.ManagedWindows.Storage;

public static class Program
{
    private static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithConsoleLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UseSkia()
            .UseConsoloniaStorage()
            .UseConsolonia()
            .UseAutoDetectedConsole()
            .WithConsoleFonts();
    }
}
