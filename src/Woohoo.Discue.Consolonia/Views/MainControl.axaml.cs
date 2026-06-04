// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Tui.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Woohoo.Audio.Player.Tui.ViewModels;

public partial class MainControl : UserControl
{
    public MainControl()
    {
        this.InitializeComponent();
    }

    private void OnExit(object sender, RoutedEventArgs e)
    {
        var lifetime = Application.Current!.ApplicationLifetime as IControlledApplicationLifetime;
        lifetime!.Shutdown();
    }

    private void PlaylistListBox_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
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
