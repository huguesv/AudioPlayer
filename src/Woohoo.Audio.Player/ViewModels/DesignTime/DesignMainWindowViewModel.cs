// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.ViewModels.DesignTime;

using Woohoo.Audio.Player.Services;

public class DesignMainWindowViewModel : MainWindowViewModel
{
    public DesignMainWindowViewModel()
        : base(new NullFilePickerService(), new NullPowerManagementService(), null, null)
    {
        this.IsTipVisible = false;
        this.AlbumPerformer = "Various Artists";
        this.AlbumTitle = "Greatest Hits";
        this.ComplexAlbumTitle = $"{this.AlbumPerformer} - {this.AlbumTitle}";
        this.CurrentTrackTitle = "Sample Track Title";
    }
}
