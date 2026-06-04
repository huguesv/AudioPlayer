// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Woohoo.Discue.Views;

using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Woohoo.Audio.Services;
using Woohoo.Discue.Contracts.Services;
using Woohoo.Discue.ViewModels;

public sealed partial class PlaybackControl : UserControl
{
    private readonly INavigationService navigationService;
    private readonly ILogger logger;
    private readonly IVisualizationProviderService visualizationProviderService;

    public PlaybackControl()
    {
        this.InitializeComponent();

        this.ViewModel = App.GetService<PlaybackViewModel>();
        this.navigationService = App.GetService<INavigationService>();
        this.logger = App.GetService<ILogger<PlaybackControl>>();
        this.visualizationProviderService = App.GetService<IVisualizationProviderService>();

        this.visualizationProviderService.DataAvailable += this.VisualizationProviderService_DataAvailable;
    }

    private void VisualizationProviderService_DataAvailable(object? sender, VisualizationEventArgs e)
    {
        this.SignalControl.SignalData = e.Visualization.Waveform;
        this.SignalControl.Invalidate();
    }

    public PlaybackViewModel ViewModel { get; }

    private void PreviousTrackButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            this.ViewModel.PreviousTrack();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error pressing previous track button.");
        }
    }

    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            this.ViewModel.PlayPause();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error pressing play/pause button.");
        }
    }

    private void NextTrackButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            this.ViewModel.NextTrack();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error pressing next track button.");
        }
    }

    private void SeekForwardButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            this.ViewModel.SeekForward();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error pressing seek forward button.");
        }
    }

    private void SeekBackwardButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            this.ViewModel.SeekBackward();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error pressing seek backward button.");
        }
    }

    private void MediaInfoButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            this.navigationService?.NavigateTo(typeof(NowPlayingViewModel).FullName!, null);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error navigating to now playing view.");
        }
    }

    private void AmplitudeButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            this.navigationService?.NavigateTo(typeof(VisualizationViewModel).FullName!, null);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error navigating to visualization view.");
        }
    }
}
