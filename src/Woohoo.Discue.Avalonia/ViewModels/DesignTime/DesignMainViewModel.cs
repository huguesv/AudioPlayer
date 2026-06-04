// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.ViewModels.DesignTime;

using Woohoo.Audio.Player.Services;

public class DesignMainViewModel : MainViewModel
{
    public DesignMainViewModel()
        : base
        (
            new DispatcherQueueService(),
            new NullFilePickerService(),
            new NullPowerManagementService(),
            new NullMediaPlayerService(),
            new HomeViewModel(new DispatcherQueueService(), new NullMruService(), new NullMediaPlayerService(), new NullAvaloniaBitmapCacheService(), new NullLogger<HomeViewModel>()),
            new PlaybackViewModel(new DispatcherQueueService(), new NullMediaPlayerService(), new NullAvaloniaBitmapCacheService(), new NullLogger<PlaybackViewModel>()),
            new NowPlayingViewModel(new DispatcherQueueService(), new NullMediaPlayerService(), new NullAvaloniaBitmapCacheService(), new NullLogger<NowPlayingViewModel>()),
            new PlaylistViewModel(new DispatcherQueueService(), new NullMediaPlayerService(), new NullLogger<PlaylistViewModel>()),
            new LyricsViewModel(new DispatcherQueueService(), new NullMediaPlayerService(), new NullLocalSettingsService(), new NullLogger<LyricsViewModel>()),
            new SettingsViewModel(new DispatcherQueueService(), new NullMediaPlayerService(), new NullLocalSettingsService(), new NullLogger<SettingsViewModel>()),
            new NullLogger<MainViewModel>())
    {
    }
}
