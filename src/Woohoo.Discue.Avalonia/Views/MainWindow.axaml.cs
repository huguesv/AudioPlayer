// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Views;

using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using Woohoo.Audio.Player.ViewModels;
using Woohoo.Audio.Services;

public partial class MainWindow : Window
{
    private readonly double[] plotPsd;
    private readonly double[] plotWave;
    private readonly double[] plotBands;
    private readonly ScottPlot.Bar[] plotBars;
    private readonly string localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly IVisualizationProviderService visualizationProviderService;

    public MainWindow()
    {
        this.InitializeComponent();

        this.AddHandler(DragDrop.DropEvent, this.OnDrop);

        if (App.Current is not null)
        {
            App.Current.ActualThemeVariantChanged += this.Current_ActualThemeVariantChanged;
        }

        this.visualizationProviderService = (App.Current as App)!.Host.Services.GetRequiredService<IVisualizationProviderService>();
        this.visualizationProviderService.DataAvailable += this.VisualizationProviderService_DataAvailable;

        this.StylePlots();

        this.plotPsd = new double[257];
        this.plotWave = new double[441];
        this.plotBands = new double[8];

        // Media Player Equalizer bands:
        // 62 Hz, 125 Hz, 250 Hz, 500 Hz, 1 kHz, 2 kHz, 4 kHz, 8 kHz, 16 kHz
        this.FftPlot.Plot.Axes.SetLimits(0, 44100 / 2, -100, 0);
        this.FftPlot.Plot.Add.Signal(this.plotPsd, 44100.0 / this.plotPsd.Length);
        this.FftPlot.Plot.Layout.Frameless();
        this.FftPlot.Plot.HideGrid();
        this.FftPlot.Plot.PlotControl?.Menu?.Clear();
        this.FftPlot.Plot.PlotControl?.UserInputProcessor.Disable();
        this.FftPlot.Refresh();

        this.WavePlot.Plot.Add.Signal(this.plotWave, 44100.0 / 1000);
        this.WavePlot.Plot.Axes.SetLimitsY(-1.0, 1.0);
        this.WavePlot.Plot.Layout.Frameless();
        this.WavePlot.Plot.HideGrid();
        this.WavePlot.Plot.PlotControl?.Menu?.Clear();
        this.WavePlot.Plot.PlotControl?.UserInputProcessor.Disable();
        this.WavePlot.Refresh();

        // TODO: Check out this Histogram sample code, it can update itself
        // and bin the data automatically: https://scottplot.net/cookbook/5.0/Histograms/HistogramBars/
        this.plotBars =
        [
            new ScottPlot.Bar() { Value = 0, Position = 1 },
            new ScottPlot.Bar() { Value = 0, Position = 2 },
            new ScottPlot.Bar() { Value = 0, Position = 3 },
            new ScottPlot.Bar() { Value = 0, Position = 4 },
            new ScottPlot.Bar() { Value = 0, Position = 5 },
            new ScottPlot.Bar() { Value = 0, Position = 6 },
            new ScottPlot.Bar() { Value = 0, Position = 7 },
            new ScottPlot.Bar() { Value = 0, Position = 8 },
        ];

        this.BandPlot.Plot.Add.Bars(this.plotBars);
        this.BandPlot.Plot.Axes.SetLimitsY(0, 100);
        this.BandPlot.Plot.Layout.Frameless();
        this.BandPlot.Plot.HideGrid();
        this.BandPlot.Plot.PlotControl?.Menu?.Clear();
        this.BandPlot.Plot.PlotControl?.UserInputProcessor.Disable();
        this.BandPlot.Refresh();
    }

    public void OnDrop(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            var files = e.DataTransfer.Items ?? Array.Empty<IDataTransferItem>();
            var filePaths = new List<string>();

            foreach (var item in files)
            {
                var raw = item.TryGetRaw(DataFormat.File);
                if (raw is IStorageFile file)
                {
                    string? path = file.TryGetLocalPath();
                    if (!string.IsNullOrEmpty(path))
                    {
                        filePaths.Add(path);
                    }
                }
                else if (raw is IStorageFolder folder)
                {
                    string? path = folder.TryGetLocalPath();
                    if (!string.IsNullOrEmpty(path))
                    {
                        filePaths.AddRange(Directory.GetFiles(path, "*", SearchOption.AllDirectories));
                    }
                }
            }

            if (filePaths.Count > 0)
            {
                _ = (this.DataContext as MainViewModel)?.OpenFileAsync(filePaths[0]);
            }
        }
    }

    private void VisualizationProviderService_DataAvailable(object? sender, VisualizationEventArgs e)
    {
        e.Visualization.CopyTo(this.plotPsd, this.plotBands, this.plotWave);

        for (int i = 0; i < this.plotBars.Length; i++)
        {
            this.plotBars[i].Value = 100.0 + this.plotBands[i];
        }

        this.FftPlot.Refresh();
        this.WavePlot.Refresh();
        this.BandPlot.Refresh();
    }

    private void Current_ActualThemeVariantChanged(object? sender, EventArgs e)
    {
        this.StylePlots();
    }

    private void StylePlots()
    {
        var regionColor = ScottPlot.Colors.Yellow;
        if (Application.Current?.TryGetResource("SystemRegionBrush", this.ActualThemeVariant, out var brush) == true)
        {
            if (brush is SolidColorBrush solidBrush)
            {
                regionColor = ScottPlot.Color.FromARGB(solidBrush.Color.A << 24 | solidBrush.Color.R << 16 | solidBrush.Color.G << 8 | solidBrush.Color.B);
            }
        }

        this.FftPlot.Plot.DataBackground.Color = regionColor;
        this.WavePlot.Plot.DataBackground.Color = regionColor;
        this.BandPlot.Plot.DataBackground.Color = regionColor;
    }

    private void Window_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Key.MediaPlayPause)
        {
            if (this.DataContext is MainViewModel vm)
            {
                vm.Playback.PlayPauseCommand.Execute(null);
                e.Handled = true;
            }
        }
        else if (e.Key == Key.MediaPreviousTrack)
        {
            if (this.DataContext is MainViewModel vm)
            {
                vm.Playback.PreviousTrackCommand.Execute(null);
                e.Handled = true;
            }
        }
        else if (e.Key == Key.MediaNextTrack)
        {
            if (this.DataContext is MainViewModel vm)
            {
                vm.Playback.NextTrackCommand.Execute(null);
                e.Handled = true;
            }
        }
        else if (e.Key == Key.F11)
        {
            if (this.WindowState == WindowState.FullScreen)
            {
                this.WindowState = WindowState.Normal;
                e.Handled = true;
            }
            else
            {
                this.WindowState = WindowState.FullScreen;
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Escape)
        {
            if (this.WindowState == WindowState.FullScreen)
            {
                this.WindowState = WindowState.Normal;
                e.Handled = true;
            }
        }
    }

    private void PlaylistListBox_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is ListBox { SelectedItem: PlaylistItemViewModel playlistItem })
        {
            playlistItem.PlayCommand.Execute(null);
        }
    }

    private void PlaylistListBox_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        if (sender is ListBox { SelectedItem: PlaylistItemViewModel playlistItem })
        {
            playlistItem.PlayCommand.Execute(null);
        }
    }

    private void RecentDiscsListBox_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (sender is ListBox { SelectedItem: HomeRecentDiscViewModel recentDisc })
        {
            if (File.Exists(recentDisc.AlbumFilePath))
            {
                _ = (this.DataContext as MainViewModel)?.OpenFileAsync(recentDisc.AlbumFilePath);
            }
        }
    }

    private void RecentDiscsListBox_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        if (sender is ListBox { SelectedItem: HomeRecentDiscViewModel recentDisc })
        {
            if (File.Exists(recentDisc.AlbumFilePath))
            {
                _ = (this.DataContext as MainViewModel)?.OpenFileAsync(recentDisc.AlbumFilePath);
            }
        }
    }
}
