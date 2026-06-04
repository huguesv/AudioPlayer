// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Services;

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Woohoo.Discue.Contracts.Services;
using Woohoo.Discue.ViewModels;
using Woohoo.Discue.Views;

internal sealed class PageService : IPageService
{
    private readonly Dictionary<string, Type> pages = [];

    public PageService()
    {
        this.Configure<HomeViewModel, HomePage>();
        this.Configure<LyricsViewModel, LyricsPage>();
        this.Configure<NowPlayingViewModel, NowPlayingPage>();
        this.Configure<PlaylistViewModel, PlaylistPage>();
        this.Configure<SettingsViewModel, SettingsPage>();
        this.Configure<VisualizationViewModel, VisualizationPage>();
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (this.pages)
        {
            if (!this.pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }

    private void Configure<TVM, TV>()
        where TVM : ObservableObject
        where TV : Page
    {
        lock (this.pages)
        {
            var key = typeof(TVM).FullName!;
            if (this.pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            var type = typeof(TV);
            if (this.pages.ContainsValue(type))
            {
                throw new ArgumentException($"This type is already configured with key {this.pages.First(p => p.Value == type).Key}");
            }

            this.pages.Add(key, type);
        }
    }
}
