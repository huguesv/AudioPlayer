// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Tui.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
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

    private void ListBox_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (this.DataContext is MainViewModel vm)
        {
            vm.PlaySelectedTrackCommand.Execute(null);
        }
    }
}
