// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core;

using System;

public static class TimeConversion
{
    public static TimeSpan FromPosition(int positionInBytes)
    {
        var sector = positionInBytes / 2352;
        var seconds = sector / 75;
        var fraction = sector % 75;
        return TimeSpan.FromSeconds(seconds).Add(TimeSpan.FromMilliseconds(fraction * (1000.0 / 75.0)));
    }
}
