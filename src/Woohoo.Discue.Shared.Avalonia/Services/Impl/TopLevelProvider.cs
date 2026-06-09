// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Shared.Avalonia.Services.Impl;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.ApplicationLifetimes;

public sealed class TopLevelProvider : ITopLevelProvider
{
    public TopLevel? GetTopLevel()
    {
        return GetTopLevel(Application.Current ?? throw new InvalidOperationException());
    }

    private static Window? GetTopLevel(Application app)
    {
        if (app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            return desktopLifetime.MainWindow;
        }

        return null;
    }
}
