// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Consolonia;

using Avalonia;
using global::Consolonia;
using global::Consolonia.Fonts;
using global::Consolonia.ManagedWindows.Storage;

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
