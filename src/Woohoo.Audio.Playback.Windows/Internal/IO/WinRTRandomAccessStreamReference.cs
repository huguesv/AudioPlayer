// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Playback.Windows.Internal.IO;

using System;
using global::Windows.Foundation;
using global::Windows.Storage.Streams;
using Woohoo.Audio.Core.IO;
using Woohoo.Audio.Core.Media;

internal sealed partial class WinRTRandomAccessStreamReference : IRandomAccessStreamReference
{
    private readonly IAlbumTrack track;
    private readonly string contentType;

    public WinRTRandomAccessStreamReference(IAlbumTrack track, string contentType)
    {
        this.track = track;
        this.contentType = contentType;
    }

    public IAsyncOperation<IRandomAccessStreamWithContentType> OpenReadAsync()
    {
        return this.OpenReadDirectAsync();
    }

    private IAsyncOperation<IRandomAccessStreamWithContentType> OpenReadDirectAsync()
    {
        var trackStream = this.track.OpenStream();

        var waveStream = new HeaderedStream(WaveHeader.Create((int)trackStream.Length), trackStream);

        IRandomAccessStreamWithContentType stream = new WinRTRandomAccessStreamWithContentType(waveStream.AsRandomAccessStream(), this.contentType);
        return Task.FromResult(stream).AsAsyncOperation();
    }

    private IAsyncOperation<IRandomAccessStreamWithContentType> OpenReadCacheAsync()
    {
        var trackStream = this.track.OpenStream();

        var waveStream = new HeaderedStream(WaveHeader.Create((int)trackStream.Length), trackStream);

        var memoryStream = new MemoryStream(capacity: (int)waveStream.Length);
        waveStream.CopyTo(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);

        IRandomAccessStreamWithContentType stream = new WinRTRandomAccessStreamWithContentType(memoryStream.AsRandomAccessStream(), this.contentType);
        return Task.FromResult(stream).AsAsyncOperation();
    }
}
