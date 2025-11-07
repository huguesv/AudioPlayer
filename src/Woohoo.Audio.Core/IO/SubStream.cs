// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.IO;

using System;
using System.IO;

public class SubStream : Stream
{
    private readonly Stream innerStream;
    private readonly long startPosition;
    private readonly long length;

    public SubStream(Stream innerStream, long startPosition, long length)
    {
        if (innerStream == null)
        {
            throw new ArgumentNullException("innerStream");
        }

        if (startPosition < 0)
        {
            throw new ArgumentOutOfRangeException("startPosition");
        }

        if (length < 0)
        {
            throw new ArgumentOutOfRangeException("length");
        }

        if (startPosition + length > innerStream.Length)
        {
            throw new ArgumentException();
        }

        this.innerStream = innerStream;
        this.startPosition = startPosition;
        this.length = length;

        // Make sure that the inner stream position is within the bounds
        // of this substream
        if (this.innerStream.Position < startPosition)
        {
            this.innerStream.Position = startPosition;
        }

        if (this.innerStream.Position > startPosition + length)
        {
            this.innerStream.Position = startPosition + length;
        }
    }

    public override bool CanRead
    {
        get { return true; }
    }

    public override bool CanSeek
    {
        get { return true; }
    }

    public override bool CanWrite
    {
        get { return false; }
    }

    public override long Length
    {
        get { return this.length; }
    }

    public override long Position
    {
        get
        {
            return this.innerStream.Position - this.startPosition;
        }

        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("value", "Position must be greater or equal to zero.");
            }

            if (value > this.length)
            {
                throw new ArgumentOutOfRangeException("value", "Position must be less or equal to the length of the stream.");
            }

            this.innerStream.Position = this.startPosition + value;
        }
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException("buffer");
        }

        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException("offset");
        }

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException("count");
        }

        if (offset + count > buffer.Length)
        {
            throw new ArgumentException();
        }

        if (this.Position < 0 || this.Position > this.length)
        {
            throw new InvalidOperationException("Current position is outside the bounds of the stream.");
        }

        var currentSubPosition = this.Position;
        var remainInStream = this.length - currentSubPosition;
        if (count > remainInStream)
        {
            count = (int)remainInStream;
        }

        if (count > 0)
        {
            return this.innerStream.Read(buffer, offset, count);
        }
        else
        {
            return 0;
        }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        long subPosition;
        switch (origin)
        {
            case SeekOrigin.Begin:
                subPosition = offset;
                break;
            case SeekOrigin.Current:
                subPosition = this.Position + offset;
                break;
            case SeekOrigin.End:
                subPosition = this.length - offset;
                break;
            default:
                throw new ArgumentOutOfRangeException("origin");
        }

        if (subPosition < 0)
        {
            throw new ArgumentOutOfRangeException("offset");
        }

        if (subPosition > this.length)
        {
            throw new ArgumentOutOfRangeException("offset");
        }

        this.Position = subPosition;

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
