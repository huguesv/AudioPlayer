// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Consolonia.Services;

using Avalonia.Controls;

public interface ITopLevelProvider
{
    TopLevel? GetTopLevel();
}
