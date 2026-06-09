// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Woohoo.Discue.Contracts.Services;

public sealed partial class CacheableImageViewModel : ObservableObject
{
    private readonly IWindowsBitmapCacheService bitmapCacheService;

    public CacheableImageViewModel(IWindowsBitmapCacheService bitmapCacheService)
    {
        ArgumentNullException.ThrowIfNull(bitmapCacheService);

        this.bitmapCacheService = bitmapCacheService;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasImage))]
    public partial string ImageUrl { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasImage))]
    public partial ImageSource? LocalImage { get; set; }

    public bool HasImage => this.LocalImage is not null;

    partial void OnImageUrlChanged(string value)
    {
        _ = this.UpdateLocalImage(value);
    }

    private async Task UpdateLocalImage(string url)
    {
        await this.LoadImageAsync(url);
    }

    private async Task LoadImageAsync(string url)
    {
        if (url.StartsWith("http:") || url.StartsWith("https:"))
        {
            var result = await this.bitmapCacheService.GetLocalImageAsync(new Uri(url));
            this.LocalImage = result;
        }
        else
        {
            this.LocalImage = null;
        }
    }
}
