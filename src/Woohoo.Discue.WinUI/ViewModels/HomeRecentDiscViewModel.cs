// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Woohoo.Audio.Services;
using Woohoo.Discue.Contracts.Services;

public sealed partial class HomeRecentDiscViewModel : ObservableObject
{
    private readonly IMruService mruService;
    private readonly IWindowsBitmapCacheService bitmapCacheService;
    private readonly ILogger logger;

    public HomeRecentDiscViewModel(
        IMruService mruService,
        IWindowsBitmapCacheService bitmapCacheService,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(mruService);
        ArgumentNullException.ThrowIfNull(bitmapCacheService);
        ArgumentNullException.ThrowIfNull(logger);

        this.mruService = mruService;
        this.bitmapCacheService = bitmapCacheService;
        this.logger = logger;

        this.AlbumArt = new CacheableImageViewModel(bitmapCacheService);
    }

    [ObservableProperty]
    public partial string AlbumFilePath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FullAlbumTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial CacheableImageViewModel AlbumArt { get; set; }

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
}
