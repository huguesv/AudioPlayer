// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Avalonia.Views;

using global::Avalonia.Controls;
using global::Avalonia.Input;
using Woohoo.Discue.Avalonia.ViewModels;

public partial class PlaylistPage : ContentPage
{
    public PlaylistPage()
    {
        this.InitializeComponent();
    }

    private void PlaylistListBox_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is ListBox { SelectedItem: PlaylistItemViewModel playlistItem })
        {
            playlistItem.PlayCommand.Execute(null);
        }
    }

    private void PlaylistListBox_KeyUp(object? sender, KeyEventArgs e)
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
}
