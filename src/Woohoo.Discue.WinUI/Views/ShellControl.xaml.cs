// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Views;

using System;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Woohoo.Discue.Contracts.Services;
using Woohoo.Discue.Helpers;
using Woohoo.Discue.ViewModels;

public sealed partial class ShellControl : UserControl, IDisposable
{
    public static readonly DependencyProperty NavigationSelectedItemProperty =
        DependencyProperty.Register(nameof(NavigationSelectedItem), typeof(object), typeof(ShellControl), new PropertyMetadata(null));

    public static readonly DependencyProperty IsBackEnabledProperty =
        DependencyProperty.Register(nameof(IsBackEnabled), typeof(bool), typeof(ShellControl), new PropertyMetadata(false));

    private readonly DisposableBag disposables = DisposableBag.Create<MainWindow>();
    private readonly INavigationService navigationService;
    private readonly INavigationViewService navigationViewService;

    public ShellControl()
    {
        this.InitializeComponent();

        this.ViewModel = App.GetService<ShellViewModel>();

        this.navigationService = App.GetService<INavigationService>();
        this.navigationViewService = App.GetService<INavigationViewService>();

        this.navigationService.Frame = this.navFrame;
        this.navigationViewService.Initialize(this.navView);

        this.navigationService.Navigated += this.OnNavigated;
        this.disposables.Add(() => this.navigationService.Navigated -= this.OnNavigated);

        this.navView.SelectedItem = this.navView.MenuItems.OfType<NavigationViewItem>().First();

        this.navigationService.NavigateTo(typeof(HomeViewModel).FullName!, null);

        WeakReferenceMessenger.Default.Register<ViewChangeMessage>(this, (r, m) =>
        {
            this.navigationService.NavigateTo(m.ViewName, null);
        });
    }

    public ShellViewModel ViewModel { get; }

    public TitleBar TitleBar => this.titleBar;

    public object? NavigationSelectedItem
    {
        get { return (object?)this.GetValue(NavigationSelectedItemProperty); }
        set { this.SetValue(NavigationSelectedItemProperty, value); }
    }

    public bool IsBackEnabled
    {
        get { return (bool)this.GetValue(IsBackEnabledProperty); }
        set { this.SetValue(IsBackEnabledProperty, value); }
    }

    public void Dispose()
    {
        this.disposables.TryDispose();
    }

    private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        this.navView.IsPaneVisible = !this.navView.IsPaneVisible;
    }

    private void TitleBar_BackRequested(TitleBar sender, object args)
    {
        if (this.navFrame.CanGoBack)
        {
            this.navFrame.GoBack();
        }
    }

    private void TitleBar_Loaded(object sender, RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(App.MainWindow!, this.titleBar.RequestedTheme);
    }

    private void TitleBar_ActualThemeChanged(FrameworkElement sender, object args)
    {
        TitleBarHelper.UpdateTitleBar(App.MainWindow!, this.titleBar.RequestedTheme);
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        this.IsBackEnabled = this.navigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            this.NavigationSelectedItem = this.navigationViewService.SettingsItem;
            return;
        }

        var selectedItem = this.navigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            this.NavigationSelectedItem = selectedItem;
        }
    }
}
