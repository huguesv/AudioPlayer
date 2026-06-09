// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Consolonia.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Woohoo.Audio.Services;

public sealed partial class VisualizationViewModel : ObservableObject
{
    private readonly IDispatcherQueue dispatcherQueue;
    private readonly IVisualizationProviderService visualizationProviderService;
    private readonly ILogger logger;

    private readonly double[] psdPlotTemp;
    private readonly double[] bandsPlotTemp;

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
        this.psdPlotTemp = new double[257];
        this.BandsPlotData = new double[8];
        this.bandsPlotTemp = new double[8];

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
        e.Visualization.CopyTo(psdPlotTemp, bandsPlotTemp, this.WavePlotData);
        for (int i = 0; i < psdPlotTemp.Length; i++)
        {
            psdPlotTemp[i] = (psdPlotTemp[i] + 50) / 50.0;
        }

        for (int i = 0; i < bandsPlotTemp.Length; i++)
        {
            bandsPlotTemp[i] = bandsPlotTemp[i] + 100;
        }

        Array.Copy(psdPlotTemp, this.PsdPlotData, psdPlotTemp.Length);
        Array.Copy(bandsPlotTemp, this.BandsPlotData, bandsPlotTemp.Length);

        this.PlotTick++;
        if (this.PlotTick == long.MaxValue)
        {
            this.PlotTick = long.MinValue;
        }
    }
}
