// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Tui.ViewModels;

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Woohoo.Audio.Services;

public sealed partial class VisualizationViewModel : ObservableObject
{
    private readonly IDispatcherQueue dispatcherQueue;
    private readonly IVisualizationProviderService visualizationProviderService;
    private readonly ILogger logger;

    public VisualizationViewModel(
        IDispatcherQueueService dispatcherQueueService,
        IVisualizationProviderService visualizationProviderService,
        ILogger<VisualizationViewModel> logger)
    {
        ArgumentNullException.ThrowIfNull(dispatcherQueueService);
        ArgumentNullException.ThrowIfNull(visualizationProviderService);
        ArgumentNullException.ThrowIfNull(logger);

        this.dispatcherQueue = dispatcherQueueService.GetDispatcherQueue();
        this.visualizationProviderService = visualizationProviderService;
        this.logger = logger;

        this.WavePlotData = new double[441];
        this.PsdPlotData = new double[257];
        this.BandsPlotData = new double[8];

        this.visualizationProviderService.DataAvailable += this.VisualizationProviderService_DataAvailable;
    }

    [ObservableProperty]
    public partial double[] WavePlotData { get; set; }

    [ObservableProperty]
    public partial double[] PsdPlotData { get; set; }

    [ObservableProperty]
    public partial double[] BandsPlotData { get; set; }

    [ObservableProperty]
    public partial long PlotTick { get; set; }

    private void VisualizationProviderService_DataAvailable(object? sender, VisualizationEventArgs e)
    {
        // Manually scale psd data from (-100,0) to (-1,1)
        // TODO: make the signal plot control customizable
        // enough so we don't have to do this.
        double[] psd = new double[this.PsdPlotData.Length];
        double[] bands = new double[this.BandsPlotData.Length];
        e.Visualization.CopyTo(psd, bands, this.WavePlotData);
        for (int i = 0; i < psd.Length; i++)
        {
            psd[i] = (psd[i] + 50) / 50.0;
        }

        for (int i = 0; i < bands.Length; i++)
        {
            bands[i] = bands[i] + 100;
        }

        Array.Copy(psd, this.PsdPlotData, psd.Length);
        Array.Copy(bands, this.BandsPlotData, bands.Length);

        this.PlotTick++;
        if (this.PlotTick == long.MaxValue)
        {
            this.PlotTick = long.MinValue;
        }
    }
}
