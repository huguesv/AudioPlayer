// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Avalonia.Views;

using CommunityToolkit.Mvvm.Messaging;
using global::Avalonia.Controls;
using Woohoo.Discue.Avalonia.ViewModels;

public partial class LyricsPage : ContentPage
{
    public LyricsPage()
    {
        this.InitializeComponent();

        WeakReferenceMessenger.Default.Register<CurrentLyricChangeMessage>(this, (r, m) =>
        {
            this.ScrollToLyricLine(m);
        });
    }

    private void ScrollToLyricLine(CurrentLyricChangeMessage m)
    {
        if (m.AutoScroll)
        {
            this.LyricsItemsRepeater
                .GetOrCreateElement(Math.Min(m.Index + 1, m.LineCount - 1))?
                .BringIntoView();
        }
    }
}
