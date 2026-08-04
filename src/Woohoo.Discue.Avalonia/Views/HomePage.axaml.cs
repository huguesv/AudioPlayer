// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Avalonia.Views;

using global::Avalonia.Controls;
using global::Avalonia.Input;
using Woohoo.Discue.Avalonia.ViewModels;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        this.InitializeComponent();
    }

    private void RecentDiscsListBox_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is ListBox { SelectedItem: HomeRecentDiscViewModel recentDisc })
        {
            if (File.Exists(recentDisc.AlbumFilePath))
            {
                _ = (this.DataContext as MainViewModel)?.OpenFileAsync(recentDisc.AlbumFilePath, CancellationToken.None);
            }
        }
    }

    private void RecentDiscsListBox_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        if (sender is ListBox { SelectedItem: HomeRecentDiscViewModel recentDisc })
        {
            if (File.Exists(recentDisc.AlbumFilePath))
            {
                _ = (this.DataContext as MainViewModel)?.OpenFileAsync(recentDisc.AlbumFilePath, CancellationToken.None);
            }
        }
    }
}
