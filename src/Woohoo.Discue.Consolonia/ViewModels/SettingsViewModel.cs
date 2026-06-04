// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Tui.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Woohoo.Audio.Player.Tui.Services;
using Woohoo.Audio.Services;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IMediaPlayerService mediaPlayerService;
    private readonly ILocalSettingsService localSettingsService;
    private readonly IThemeService themeService;
    private readonly ILogger logger;
    private readonly IDispatcherQueue dispatcherQueue;

    private bool isLoading;

    public SettingsViewModel(
        IDispatcherQueueService dispatcherQueueService,
        IMediaPlayerService mediaPlayerService,
        ILocalSettingsService localSettingsService,
        IThemeService themeService,
        ILogger<SettingsViewModel> logger)
    {
        ArgumentNullException.ThrowIfNull(dispatcherQueueService);
        ArgumentNullException.ThrowIfNull(mediaPlayerService);
        ArgumentNullException.ThrowIfNull(localSettingsService);
        ArgumentNullException.ThrowIfNull(themeService);
        ArgumentNullException.ThrowIfNull(logger);

        this.mediaPlayerService = mediaPlayerService;
        this.localSettingsService = localSettingsService;
        this.themeService = themeService;
        this.logger = logger;
        this.dispatcherQueue = dispatcherQueueService.GetDispatcherQueue();

        this.AboutInfo = new();

        this.isLoading = true;
        try
        {
            this.QueryMetadataOnline = this.localSettingsService.ReadSetting<bool?>(KnownSettingKeys.QueryMetadataOnline) ?? false;
            this.QueryLyricsOnline = this.localSettingsService.ReadSetting<bool?>(KnownSettingKeys.QueryLyricsOnline) ?? false;
            this.QueryLyricsOffline = this.localSettingsService.ReadSetting<bool?>(KnownSettingKeys.QueryLyricsOffline) ?? false;
            this.QueryLyricsOfflineDatabasePath = this.localSettingsService.ReadSetting<string>(KnownSettingKeys.QueryLyricsOfflineDatabasePath) ?? string.Empty;
            this.LyricsAutoScroll = this.localSettingsService.ReadSetting<bool?>(KnownSettingKeys.LyricsAutoScroll) ?? true;
            this.AudioEngine = Enum.Parse<AudioEngineType>(this.localSettingsService.ReadSetting<string>(KnownSettingKeys.AudioEngine) ?? AudioEngineType.WindowsMediaPlayer.ToString());
            this.SelectedTheme = this.localSettingsService.ReadSetting<string>(KnownSettingKeys.Theme) ?? KnownThemes.ModernDark;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error loading settings.");
        }
        finally
        {
            this.isLoading = false;
        }
    }

    public SettingsAboutInformationViewModel AboutInfo { get; }

    public string AudioEngineDisplayName => this.mediaPlayerService.AudioEngineDisplayName;

    [ObservableProperty]
    public partial bool QueryMetadataOnline { get; set; }

    [ObservableProperty]
    public partial bool QueryLyricsOnline { get; set; }

    [ObservableProperty]
    public partial bool QueryLyricsOffline { get; set; }

    [ObservableProperty]
    public partial string QueryLyricsOfflineDatabasePath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool LyricsAutoScroll { get; set; }

    [ObservableProperty]
    public partial AudioEngineType AudioEngine { get; set; } = AudioEngineType.Sdl3;

    [ObservableProperty]
    public partial string SelectedTheme { get; set; } = KnownThemes.ModernDark;

    [RelayCommand]
    public void ToggleQueryMetadataOnline()
    {
        this.QueryMetadataOnline = !this.QueryMetadataOnline;
    }

    [RelayCommand]
    public void ToggleQueryLyricsOnline()
    {
        this.QueryLyricsOnline = !this.QueryLyricsOnline;
    }

    [RelayCommand]
    public void ToggleQueryLyricsOffline()
    {
        this.QueryLyricsOffline = !this.QueryLyricsOffline;
    }

    [RelayCommand]
    public void ToggleLyricsAutoScroll()
    {
        this.LyricsAutoScroll = !this.LyricsAutoScroll;
    }

    [RelayCommand]
    public void SelectTheme(string theme)
    {
        this.SelectedTheme = theme;
        this.themeService.SelectedTheme = theme;
    }

    partial void OnQueryMetadataOnlineChanged(bool value)
    {
        if (this.isLoading)
        {
            return;
        }

        this.localSettingsService.SaveSetting(KnownSettingKeys.QueryMetadataOnline, value);
    }

    partial void OnQueryLyricsOnlineChanged(bool value)
    {
        if (this.isLoading)
        {
            return;
        }

        this.localSettingsService.SaveSetting(KnownSettingKeys.QueryLyricsOnline, value);
    }

    partial void OnQueryLyricsOfflineChanged(bool value)
    {
        if (this.isLoading)
        {
            return;
        }

        this.localSettingsService.SaveSetting(KnownSettingKeys.QueryLyricsOffline, value);
    }

    partial void OnQueryLyricsOfflineDatabasePathChanged(string value)
    {
        if (this.isLoading)
        {
            return;
        }

        this.localSettingsService.SaveSetting(KnownSettingKeys.QueryLyricsOfflineDatabasePath, value);
    }

    partial void OnLyricsAutoScrollChanged(bool value)
    {
        if (this.isLoading)
        {
            return;
        }

        this.localSettingsService.SaveSetting(KnownSettingKeys.LyricsAutoScroll, value);
    }

    partial void OnSelectedThemeChanged(string value)
    {
        if (this.isLoading)
        {
            return;
        }

        this.localSettingsService.SaveSetting(KnownSettingKeys.Theme, value);
    }

    [RelayCommand]
    private void SwitchAudioEngine(int engineType)
    {
        try
        {
            this.SwitchAudioEngine((AudioEngineType)engineType);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    private void SwitchAudioEngine(AudioEngineType priority)
    {
        if (this.AudioEngine != priority)
        {
            this.AudioEngine = priority;
            this.localSettingsService.SaveSetting(KnownSettingKeys.AudioEngine, priority.ToString());
        }
    }
}
