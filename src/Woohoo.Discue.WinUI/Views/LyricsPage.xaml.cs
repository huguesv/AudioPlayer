// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Views;

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.UI.Text;
using Woohoo.Discue.ViewModels;

public sealed partial class LyricsPage : Page
{
    public LyricsPage()
    {
        this.ViewModel = App.GetService<LyricsViewModel>();
        this.InitializeComponent();

        WeakReferenceMessenger.Default.Register<CurrentLyricChangeMessage>(this, (r, m) =>
        {
            this.ScrollToLyricLine(m);
        });
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);

        WeakReferenceMessenger.Default.Unregister<CurrentLyricChangeMessage>(this);
    }

    public LyricsViewModel ViewModel { get; }

    public static Brush GetLyricBrush(bool isActive)
    {
        string resourceKey = isActive ? "AccentTextFillColorPrimaryBrush" : "TextFillColorPrimaryBrush";
        return (Brush)App.Current.Resources[resourceKey];
    }

    public static FontStyle GetLyricFontStyle(bool isActive)
    {
        return isActive ? FontStyle.Italic : FontStyle.Normal;
    }

    public static FontWeight GetLyricFontWeight(bool isActive)
    {
        return isActive ? FontWeights.SemiBold : FontWeights.Normal;
    }

    private void ScrollToLyricLine(CurrentLyricChangeMessage m)
    {
        if (m.AutoScroll)
        {
            this.LyricsItemsRepeater
                .TryGetElement(Math.Min(m.Index + 1, m.LineCount - 1))?
                .StartBringIntoView();
        }
    }
}
