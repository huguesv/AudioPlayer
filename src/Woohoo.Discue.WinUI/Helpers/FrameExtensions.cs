// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Helpers;

using Microsoft.UI.Xaml.Controls;
using Woohoo.Discue.Contracts.ViewModel;

internal static class FrameExtensions
{
    public static INavigationAware? GetPageNavigationAware(this Frame frame) => frame?.Content as INavigationAware;
}
