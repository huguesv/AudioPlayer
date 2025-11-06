// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

public partial class AlbumArtViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Url { get; set; }

    [ObservableProperty]
    public partial bool IsPrimary { get; set; }
}
