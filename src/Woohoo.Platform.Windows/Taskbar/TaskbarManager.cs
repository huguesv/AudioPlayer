// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Platform.Windows.Taskbar;

using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using global::Windows.Win32.Foundation;
using global::Windows.Win32.UI.Shell;

[SupportedOSPlatform("windows6.1")]
public class TaskbarManager
{
    private static readonly Lazy<TaskbarManager> InstanceLazy = new(() => new TaskbarManager());

    private static readonly Lazy<ITaskbarList4> TaskbarListLazy = new(() =>
    {
        var taskbarList = (ITaskbarList4)new TaskbarList();
        taskbarList.HrInit();
        return taskbarList;
    });

    private readonly Lazy<HWND> ownerWindowHandleLazy = new(() =>
    {
        var currentProcess = Process.GetCurrentProcess();
        if (currentProcess is null || currentProcess.MainWindowHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Window for current process was not found.");
        }

        return new HWND(currentProcess.MainWindowHandle);
    });

    private TaskbarManager()
    {
    }

    public static TaskbarManager Instance => InstanceLazy.Value;

    public void SetProgressValue(int currentValue, int maximumValue)
    {
        TaskbarListLazy.Value.SetProgressValue(this.ownerWindowHandleLazy.Value, Convert.ToUInt32(currentValue), Convert.ToUInt32(maximumValue));
    }

    public void SetProgressValue(int currentValue, int maximumValue, IntPtr windowHandle)
    {
        TaskbarListLazy.Value.SetProgressValue(new HWND(windowHandle), Convert.ToUInt32(currentValue), Convert.ToUInt32(maximumValue));
    }

    public void SetProgressState(TaskbarProgressBarState state)
    {
        TaskbarListLazy.Value.SetProgressState(this.ownerWindowHandleLazy.Value, (TBPFLAG)state);
    }

    public void SetProgressState(TaskbarProgressBarState state, IntPtr windowHandle)
    {
        TaskbarListLazy.Value.SetProgressState(new HWND(windowHandle), (TBPFLAG)state);
    }
}
