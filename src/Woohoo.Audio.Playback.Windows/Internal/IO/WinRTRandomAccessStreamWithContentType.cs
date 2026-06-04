// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Playback.Windows.Internal.IO;

using global::Windows.Foundation;
using global::Windows.Storage.Streams;

internal sealed partial class WinRTRandomAccessStreamWithContentType : IRandomAccessStreamWithContentType
{
    private readonly IRandomAccessStream innerStream;

    public WinRTRandomAccessStreamWithContentType(IRandomAccessStream innerStream, string contentType)
    {
        this.innerStream = innerStream;
        this.ContentType = contentType;
    }

    public bool CanRead => this.innerStream.CanRead;

    public bool CanWrite => this.innerStream.CanWrite;

    public ulong Position => this.innerStream.Position;

    public ulong Size { get => this.innerStream.Size; set => this.innerStream.Size = value; }

    public string ContentType { get; }

    public IRandomAccessStream CloneStream()
    {
        return this.innerStream.CloneStream();
    }

    public void Dispose()
    {
        this.innerStream.Dispose();
    }

    public IAsyncOperation<bool> FlushAsync()
    {
        return this.innerStream.FlushAsync();
    }

    public IInputStream GetInputStreamAt(ulong position)
    {
        return this.innerStream.GetInputStreamAt(position);
    }

    public IOutputStream GetOutputStreamAt(ulong position)
    {
        return this.innerStream.GetOutputStreamAt(position);
    }

    public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
    {
        return this.innerStream.ReadAsync(buffer, count, options);
    }

    public void Seek(ulong position)
    {
        this.innerStream.Seek(position);
    }

    public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
    {
        return this.innerStream.WriteAsync(buffer);
    }
}
