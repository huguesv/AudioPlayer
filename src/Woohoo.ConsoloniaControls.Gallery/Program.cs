// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ConsoloniaControls.Gallery;

using Avalonia;
using Consolonia;

internal class Program
{
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

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .LogToException()
            .UseConsolonia()
            .UseAutoDetectedConsole()
            .ThrowOnErrors();
    }
}
