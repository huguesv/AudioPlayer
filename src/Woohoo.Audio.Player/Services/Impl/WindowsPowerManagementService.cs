// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Services;

using System.Runtime.Versioning;
using Woohoo.Platform.Windows.PowerManagement;

[SupportedOSPlatform("windows5.1.2600")]
internal class WindowsPowerManagementService : IPowerManagementService
{
    public void PreventSleep()
    {
        ThreadExecutionManager.SetState(ThreadExecutionState.Continuous | ThreadExecutionState.DisplayRequired);
    }

    public void RestoreSleep()
    {
        ThreadExecutionManager.SetState(ThreadExecutionState.Continuous);
    }
}
