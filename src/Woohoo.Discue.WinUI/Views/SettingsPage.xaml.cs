// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Views;

using Microsoft.UI.Xaml.Controls;
using Woohoo.Discue.ViewModels;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.ViewModel = App.GetService<SettingsViewModel>();
        this.InitializeComponent();
    }

    public SettingsViewModel ViewModel { get; }
}
