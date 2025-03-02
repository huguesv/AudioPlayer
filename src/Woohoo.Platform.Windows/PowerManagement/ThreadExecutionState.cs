// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Platform.Windows.PowerManagement;

using global::Windows.Win32.System.Power;

[Flags]
public enum ThreadExecutionState : uint
{
    AwayModeRequired = EXECUTION_STATE.ES_AWAYMODE_REQUIRED,
    Continuous = EXECUTION_STATE.ES_CONTINUOUS,
    DisplayRequired = EXECUTION_STATE.ES_DISPLAY_REQUIRED,
    SystemRequired = EXECUTION_STATE.ES_SYSTEM_REQUIRED,

    // Legacy flag, should not be used.
    // ES_USER_PRESENT = 0x00000004
}
