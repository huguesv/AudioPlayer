// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Avalonia.Services.Impl;

using System;
using global::Avalonia.Controls;
using Woohoo.Discue.Avalonia.Services;

internal class TopLevelProvider : ITopLevelProvider
{
    public TopLevel? GetTopLevel()
    {
        return App.GetTopLevel(App.Current ?? throw new InvalidOperationException());
    }
}
