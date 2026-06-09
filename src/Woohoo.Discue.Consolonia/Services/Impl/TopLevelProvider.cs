// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Consolonia.Services.Impl;

using Avalonia.Controls;

internal class TopLevelProvider : ITopLevelProvider
{
    public TopLevel? GetTopLevel()
    {
        return App.GetTopLevel(App.Current ?? throw new InvalidOperationException());
    }
}
