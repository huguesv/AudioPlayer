// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Consolonia;

using Avalonia;
using global::Consolonia;
using global::Consolonia.Fonts;
using global::Consolonia.ManagedWindows.Storage;

public static class Program
{
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .LogToException()
            .UseSkia()
            .UseConsoloniaStorage()
            .UseConsolonia()
            .UseAutoDetectedConsole()
            .WithConsoleFonts()
            .ThrowOnErrors();
    }

    [STAThread]
    private static void Main(string[] args)
    {
        // Bug in Consolonia? Without this force initialization of Dispatcher.UIThread,
        // the UIThread is initialized in MTA and is a background thread.
        var dispatcher = Avalonia.Threading.Dispatcher.UIThread;

        TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            ThreadPool.QueueUserWorkItem(state =>
                throw new InvalidOperationException("An unobserved task exception occurred.", eventArgs.Exception));
        };

        BuildAvaloniaApp()
            .StartWithConsoleLifetime(args);
    }
}
