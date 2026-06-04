// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Services;

using System.Diagnostics.CodeAnalysis;
using Microsoft.UI.Xaml.Controls;
using Woohoo.Audio.Services;
using Woohoo.Discue.Contracts.Services;
using Woohoo.Discue.Helpers;
using Woohoo.Discue.ViewModels;

internal sealed class NavigationViewService : INavigationViewService
{
    private readonly INavigationService navigationService;

    private readonly IPageService pageService;

    private NavigationView? navigationView;

    public NavigationViewService(INavigationService navigationService, IPageService pageService)
    {
        this.navigationService = navigationService;
        this.pageService = pageService;
    }

    public IList<object>? MenuItems => this.navigationView?.MenuItems;

    public object? SettingsItem => this.navigationView?.SettingsItem;

    [MemberNotNull(nameof(NavigationViewService.navigationView))]
    public void Initialize(NavigationView navigationView)
    {
        this.navigationView = navigationView;
        this.navigationView.BackRequested += this.OnBackRequested;
        this.navigationView.ItemInvoked += this.OnItemInvoked;
    }

    public void UnregisterEvents()
    {
        if (this.navigationView != null)
        {
            this.navigationView.BackRequested -= this.OnBackRequested;
            this.navigationView.ItemInvoked -= this.OnItemInvoked;
        }
    }

    public NavigationViewItem? GetSelectedItem(Type pageType)
    {
        if (this.navigationView != null)
        {
            return this.GetSelectedItem(this.navigationView.MenuItems, pageType) ?? this.GetSelectedItem(this.navigationView.FooterMenuItems, pageType);
        }

        return null;
    }

    private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args) => this.navigationService.GoBack();

    private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
            this.navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
        }
        else
        {
            var selectedItem = args.InvokedItemContainer as NavigationViewItem;

            if (selectedItem?.GetValue(NavigationHelper.NavigateToProperty) is string pageKey)
            {
                this.navigationService.NavigateTo(pageKey);
            }
        }
    }

    private NavigationViewItem? GetSelectedItem(IEnumerable<object> menuItems, Type pageType)
    {
        foreach (var item in menuItems.OfType<NavigationViewItem>())
        {
            if (this.IsMenuItemForPageType(item, pageType))
            {
                return item;
            }

            var selectedChild = this.GetSelectedItem(item.MenuItems, pageType);
            if (selectedChild != null)
            {
                return selectedChild;
            }
        }

        return null;
    }

    private bool IsMenuItemForPageType(NavigationViewItem menuItem, Type sourcePageType)
    {
        if (menuItem.GetValue(NavigationHelper.NavigateToProperty) is string pageKey)
        {
            return this.pageService.GetPageType(pageKey) == sourcePageType;
        }

        return false;
    }
}
