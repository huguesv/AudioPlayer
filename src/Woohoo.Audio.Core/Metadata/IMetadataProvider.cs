// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Metadata;

using System.Threading.Tasks;

public interface IMetadataProvider
{
    Task<AlbumMetadata?> QueryAsync(int audioTrackCount, string toc, CancellationToken cancellationToken);
}
