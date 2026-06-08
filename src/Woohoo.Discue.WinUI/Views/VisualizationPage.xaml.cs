// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Views;

using Microsoft.UI.Xaml.Controls;
using Woohoo.Audio.Services;
using Woohoo.Discue.ViewModels;

public sealed partial class VisualizationPage : Page
{
    private readonly IVisualizationProviderService visualizationProviderService;

    public VisualizationPage()
    {
        this.InitializeComponent();

        this.ViewModel = App.GetService<VisualizationViewModel>();
        this.visualizationProviderService = App.GetService<IVisualizationProviderService>();

        this.visualizationProviderService.DataAvailable += this.VisualizationProviderService_DataAvailable;
    }

    public VisualizationViewModel ViewModel { get; }

    private void VisualizationProviderService_DataAvailable(object? sender, VisualizationEventArgs e)
    {
        this.SignalControl.SignalData = e.Visualization.Waveform;
        this.SignalControl.Invalidate();
    }
}
