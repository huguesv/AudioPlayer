// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue;

using System;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinUIEx;
using Woohoo.Audio.Services;

public sealed partial class MainWindow : WindowEx, IDisposable
{
    private readonly IMediaPlayerService mediaPlayerService;

    public MainWindow()
    {
        this.InitializeComponent();

        // Set a unique persistence ID for automatic state management
        this.PersistenceId = this.GetType().FullName;

        // Extend the content into the title bar and hide the default title bar
        this.ExtendsContentIntoTitleBar = true;

        this.AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Standard;

        // Set the custom title bar
        this.SetTitleBar(this.shellControl.TitleBar);

        this.mediaPlayerService = App.GetService<IMediaPlayerService>();
    }

    public void Dispose()
    {
        this.mediaPlayerService.Shutdown();
    }

    private void WindowEx_Closed(object sender, WindowEventArgs args)
    {
        this.Dispose();
    }
}
