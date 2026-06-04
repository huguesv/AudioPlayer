// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Woohoo.Discue.ViewModels;

public sealed partial class PlaylistPage : Page
{
    public PlaylistPage()
    {
        this.ViewModel = App.GetService<PlaylistViewModel>();
        this.InitializeComponent();
    }

    public PlaylistViewModel ViewModel { get; }

    public static Brush GetPlaylistTrackBrush(bool isActive)
    {
        string resourceKey = isActive ? "AccentTextFillColorPrimaryBrush" : "TextFillColorPrimaryBrush";
        return (Brush)App.Current.Resources[resourceKey];
    }

    private void Grid_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is PlaylistItemViewModel playlistItem)
        {
            playlistItem.PlayCommand.Execute(null);
        }
    }
}
