// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Sdl3;

using System;

public class SdlException : Exception
{
    public SdlException(string? message)
        : base(message)
    {
    }

    public SdlException(string functionName, string? error)
        : this(string.Format($"{functionName} failed: {0}", error ?? "unknown error"))
    {
    }

    public static void Throw(string functionName)
    {
        throw new SdlException(functionName, SDL.SDL3.SDL_GetError());
    }
}
