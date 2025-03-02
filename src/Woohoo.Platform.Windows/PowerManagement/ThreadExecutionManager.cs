// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Platform.Windows.PowerManagement;

using System.Runtime.Versioning;
using global::Windows.Win32;
using global::Windows.Win32.System.Power;

[SupportedOSPlatform("windows5.1.2600")]
public static class ThreadExecutionManager
{
    public static ThreadExecutionState SetState(ThreadExecutionState state)
    {
        return (ThreadExecutionState)PInvoke.SetThreadExecutionState((EXECUTION_STATE)state);
    }
}
