// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Woohoo.Discue.ViewModels;

public sealed partial class NowPlayingPage : Page
{
    public NowPlayingPage()
    {
        this.ViewModel = App.GetService<NowPlayingViewModel>();
        this.InitializeComponent();
    }

    public NowPlayingViewModel ViewModel { get; }

    private void Border_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        this.artShadow.Receivers.Add(artShadowCastGrid);
    }
}
