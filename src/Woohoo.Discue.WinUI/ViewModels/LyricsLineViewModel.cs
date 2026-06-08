// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;

public sealed partial class LyricsLineViewModel : ObservableObject
{
    public required TimeSpan Timestamp { get; init; }

    public required string Text { get; init; }

    [ObservableProperty]
    public partial bool IsCurrent { get; set; }
}
