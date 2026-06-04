// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Views;

using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;
using Woohoo.Discue.ViewModels;

public sealed partial class HomePage : Page
{
    private readonly ILogger logger;

    public HomePage()
    {
        this.ViewModel = App.GetService<HomeViewModel>();
        this.logger = App.GetService<ILogger<HomePage>>();
        this.InitializeComponent();
    }

    public HomeViewModel ViewModel { get; }

    private void OpenFileButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            _ = this.OpenFileAsync(button);
        }
    }

    private async Task OpenFileAsync(Button button)
    {
        try
        {
            button.IsEnabled = false;

            var picker = new FileOpenPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId)
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder,
                ViewMode = PickerViewMode.List,
            };

            picker.FileTypeFilter.Add(".cue");
            picker.FileTypeFilter.Add(".zip");
            picker.FileTypeFilter.Add(".chd");

            var file = await picker.PickSingleFileAsync();

            if (file is not null)
            {
                this.ViewModel.LoadAlbum(file.Path);
            }

        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error opening file.");
        }
        finally
        {
            button.IsEnabled = true;
        }
    }

    private void Album_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is HomeRecentDiscViewModel disc)
        {
            disc.LoadAlbumCommand.Execute(null);
        }
    }
}
