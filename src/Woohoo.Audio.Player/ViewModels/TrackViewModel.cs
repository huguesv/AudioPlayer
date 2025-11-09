// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Woohoo.Audio.Core.Cue;
using Woohoo.Audio.Core.IO;

public partial class TrackViewModel : ViewModelBase
{
    public TrackViewModel()
    {
        this.FileName = string.Empty;
        this.Title = string.Empty;
        this.Performer = string.Empty;
        this.Songwriter = string.Empty;
    }

    public required IMusicContainer Container { get; init; }

    public required CueSheet CueSheet { get; init; }

    [ObservableProperty]
    public partial string FileName { get; set; }

    [ObservableProperty]
    public partial bool FileNotFound { get; set; }

    [ObservableProperty]
    public partial int TrackOffset { get; set; }

    [ObservableProperty]
    public partial int TrackSize { get; set; }

    [ObservableProperty]
    public partial int TrackNumber { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; }

    [ObservableProperty]
    public partial string Performer { get; set; }

    [ObservableProperty]
    public partial string Songwriter { get; set; }

    [ObservableProperty]
    public partial bool IsCurrentTrack { get; set; }
}
