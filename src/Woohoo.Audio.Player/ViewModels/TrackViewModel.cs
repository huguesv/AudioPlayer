// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public partial class TrackViewModel : ViewModelBase
{
    [ObservableProperty]
    private string fileName;

    [ObservableProperty]
    private long fileSize;

    [ObservableProperty]
    private int trackNumber;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string performer;

    [ObservableProperty]
    private string songwriter;

    public TrackViewModel()
    {
        this.FileName = string.Empty;
        this.Title = string.Empty;
        this.Performer = string.Empty;
        this.Songwriter = string.Empty;
    }
}
