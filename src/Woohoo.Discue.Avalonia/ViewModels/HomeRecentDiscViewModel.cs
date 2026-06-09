// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Avalonia.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using global::Avalonia.Media;
using Microsoft.Extensions.Logging;
using Woohoo.Audio.Services;
using Woohoo.Discue.Shared.Avalonia.Services;

public sealed partial class HomeRecentDiscViewModel : ObservableObject
{
    private readonly IMruService mruService;
    private readonly IAvaloniaBitmapCacheService bitmapCacheService;
    private readonly ILogger logger;

    public HomeRecentDiscViewModel(
        IMruService mruService,
        IAvaloniaBitmapCacheService bitmapCacheService,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(mruService);
        ArgumentNullException.ThrowIfNull(bitmapCacheService);
        ArgumentNullException.ThrowIfNull(logger);

        this.mruService = mruService;
        this.bitmapCacheService = bitmapCacheService;
        this.logger = logger;
    }

    [ObservableProperty]
    public partial string AlbumFilePath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FullAlbumTitle { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAlbumArt))]
    public partial string AlbumArtUrl { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAlbumArt))]
    public partial IImage? AlbumArt { get; set; }

    public bool HasAlbumArt => this.AlbumArt is not null;

    [RelayCommand(CanExecute = nameof(CanLoadAlbum))]
    public void LoadAlbum()
    {
        try
        {
            WeakReferenceMessenger.Default.Send(new LoadAlbumMessage { AlbumFilePath = this.AlbumFilePath });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    public void RemoveFromRecent()
    {
        try
        {
            this.mruService.RemoveItem(this.AlbumFilePath);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    [RelayCommand]
    public void ClearAllFromRecent()
    {
        try
        {
            this.mruService.ClearItems();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing command.");
        }
    }

    private bool CanLoadAlbum() => File.Exists(this.AlbumFilePath);

    partial void OnAlbumArtUrlChanged(string value)
    {
        _ = this.UpdateAlbumArt(value);
    }

    private async Task UpdateAlbumArt(string url)
    {
        await this.LoadAlbumArtAsync(url);
    }

    private async Task LoadAlbumArtAsync(string url)
    {
        if (url.StartsWith("http:") || url.StartsWith("https:"))
        {
            var result = await this.bitmapCacheService.GetLocalImageAsync(new Uri(url));
            this.AlbumArt = result;
        }
        else
        {
            this.AlbumArt = null;
        }
    }
}
