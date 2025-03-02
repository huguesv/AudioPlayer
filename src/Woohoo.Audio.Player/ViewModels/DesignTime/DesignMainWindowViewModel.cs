// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.ViewModels.DesignTime;

using Woohoo.Audio.Player.Services;

public class DesignMainWindowViewModel : MainWindowViewModel
{
    public DesignMainWindowViewModel()
        : base(new NullFilePickerService(), new NullPowerManagementService())
    {
    }
}
