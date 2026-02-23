// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Tui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public partial class LyricLineViewModel : ViewModelBase
{
    public required TimeSpan Timestamp { get; init; }

    public required string Text { get; init; }

    [ObservableProperty]
    public partial bool IsCurrent { get; set; }
}
