// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Avalonia.Helpers;

using global::Avalonia.Controls;

internal sealed class WindowSettings
{
    public double X { get; set; }

    public double Y { get; set; }

    public double Width { get; set; } = 800;

    public double Height { get; set; } = 600;

    public WindowState WindowState { get; set; } = WindowState.Normal;
}
