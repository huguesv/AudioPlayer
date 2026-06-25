// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Media;

using System;

public sealed class MediaLoadException : Exception
{
    public MediaLoadException(string message)
        : base(message)
    {
    }
}
