// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.Chd;

using System.Diagnostics.CodeAnalysis;

public readonly struct ChdMetadataTag
{
    public ChdMetadataTag(char a, char b, char c, char d)
    {
        this.Text = $"{a}{b}{c}{d}";
        this.Number = ((uint)a << 24) | ((uint)b << 16) | ((uint)c << 8) | (uint)d;
    }

    public ChdMetadataTag(uint val)
    {
        this.Text = $"{(char)((val >> 24) & 0xFF)}{(char)((val >> 16) & 0xFF)}{(char)((val >> 8) & 0xFF)}{(char)((val >> 0) & 0xFF)}";
        this.Number = val;
    }

    public string Text { get; }

    public uint Number { get; }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is ChdMetadataTag tagObj)
        {
            return this.Number == tagObj.Number;
        }

        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return this.Text.GetHashCode();
    }

    public static bool operator ==(ChdMetadataTag left, ChdMetadataTag right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ChdMetadataTag left, ChdMetadataTag right)
    {
        return !(left == right);
    }
}
