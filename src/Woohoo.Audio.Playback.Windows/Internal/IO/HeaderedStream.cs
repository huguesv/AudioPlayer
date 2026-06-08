// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Playback.Windows.Internal.IO;

using System;

internal sealed class HeaderedStream : Stream
{
    private readonly Stream headerStream;
    private readonly Stream innerStream;

    public HeaderedStream(byte[] header, Stream innerStream)
    {
        this.headerStream = new MemoryStream(header);
        this.innerStream = innerStream;
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => this.headerStream.Length + this.innerStream.Length;

    public override long Position { get; set; }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int bytesReadFromHeader = 0;
        if (this.Position < this.headerStream.Length)
        {
            this.headerStream.Position = this.Position;
            bytesReadFromHeader = this.headerStream.Read(buffer, offset, (int)Math.Min(count, this.headerStream.Length - this.Position));
            offset += bytesReadFromHeader;
            count -= bytesReadFromHeader;
            this.Position += bytesReadFromHeader;
        }

        if (count > 0)
        {
            long innerPosition = this.Position - this.headerStream.Length;
            this.innerStream.Position = innerPosition;
            int bytesReadFromInner = this.innerStream.Read(buffer, offset, count);
            this.Position += bytesReadFromInner;
            return bytesReadFromHeader + bytesReadFromInner;
        }

        return bytesReadFromHeader;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        this.Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => this.Position + offset,
            SeekOrigin.End => this.Length + offset,
            _ => throw new ArgumentException("Invalid seek origin.", nameof(origin)),
        };
        return this.Position;
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
}
