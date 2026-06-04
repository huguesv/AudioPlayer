// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Services;

using System;
using Microsoft.UI.Xaml;
using Woohoo.Audio.Services;

public sealed class VisualizationProviderService : IVisualizationProviderService
{
    private readonly IMediaPlayerService mediaPlayerService;
    private readonly DispatcherTimer visualizationTimer;

    public VisualizationProviderService(IMediaPlayerService mediaPlayerService)
    {
        ArgumentNullException.ThrowIfNull(mediaPlayerService);

        this.mediaPlayerService = mediaPlayerService;

        this.visualizationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(32)
        };
        this.visualizationTimer.Tick += this.VisualizationTimer_Tick;
        this.visualizationTimer.Start();
    }

    public event EventHandler<VisualizationEventArgs>? DataAvailable;

    private void VisualizationTimer_Tick(object? sender, object e)
    {
        this.DataAvailable?.Invoke(this, new VisualizationEventArgs(this.mediaPlayerService.Visualization));
    }
}
