// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Services;

using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.WinUI.Animations;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Woohoo.Audio.Services;
using Woohoo.Discue.Contracts.Services;
using Woohoo.Discue.Contracts.ViewModel;
using Woohoo.Discue.Helpers;

// For more information on navigation between pages see
// https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/navigation.md
internal sealed class NavigationService : INavigationService
{
    private readonly IPageService pageService;
    private object? lastParameterUsed;
    private Frame? frame;

    public NavigationService(IPageService pageService)
    {
        this.pageService = pageService;
    }

    public event NavigatedEventHandler? Navigated;

    public Frame? Frame
    {
        get
        {
            if (this.frame == null)
            {
                this.frame = App.MainWindow?.Content as Frame;
                this.RegisterFrameEvents();
            }

            return this.frame;
        }

        set
        {
            this.UnregisterFrameEvents();
            this.frame = value;
            this.RegisterFrameEvents();
        }
    }

    [MemberNotNullWhen(true, nameof(Frame), nameof(frame))]
    public bool CanGoBack => this.Frame != null && this.Frame.CanGoBack;

    public bool GoBack()
    {
        if (this.CanGoBack)
        {
            var vmBeforeNavigation = this.frame.GetPageViewModel();
            this.frame.GoBack();
            if (vmBeforeNavigation is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedFrom();
            }

            return true;
        }

        return false;
    }

    public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false)
    {
        var pageType = this.pageService.GetPageType(pageKey);

        if (this.frame != null && (this.frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(this.lastParameterUsed))))
        {
            this.frame.Tag = clearNavigation;
            var vmBeforeNavigation = this.frame.GetPageViewModel();
            var navigated = this.frame.Navigate(pageType, parameter);
            if (navigated)
            {
                this.lastParameterUsed = parameter;
                if (vmBeforeNavigation is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedFrom();
                }
            }

            return navigated;
        }

        return false;
    }

    public void SetListDataItemForNextConnectedAnimation(object item)
        => this.Frame?.SetListDataItemForNextConnectedAnimation(item);

    private void RegisterFrameEvents()
    {
        if (this.frame != null)
        {
            this.frame.Navigated += this.OnNavigated;
        }
    }

    private void UnregisterFrameEvents()
    {
        if (this.frame != null)
        {
            this.frame.Navigated -= this.OnNavigated;
        }
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is Frame frame)
        {
            var clearNavigation = (bool)frame.Tag;
            if (clearNavigation)
            {
                frame.BackStack.Clear();
            }

            if (frame.GetPageViewModel() is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(e.Parameter);
            }

            this.Navigated?.Invoke(sender, e);
        }
    }
}
