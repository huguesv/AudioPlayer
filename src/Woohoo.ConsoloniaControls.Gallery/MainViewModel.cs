// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ConsoloniaControls.Gallery;

using System;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;

internal partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    public partial double[] PlotData { get; set; }

    [ObservableProperty]
    public partial long PlotTick { get; set; }

    [ObservableProperty]
    public partial int PlotDataLength { get; set; }

    public MainViewModel()
    {
        this.PlotDataLength = 100;
        this.FillPlotData();
    }

    partial void OnPlotDataLengthChanged(int value)
    {
        this.FillPlotData();
    }

    [MemberNotNull(nameof(PlotData))]
    private void FillPlotData()
    {
        this.PlotData = new double[this.PlotDataLength];
        for (int i = 0; i < this.PlotData.Length; i++)
        {
            this.PlotData[i] = Math.Sin(i / 6.0);
        }
    }
}
