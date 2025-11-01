// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Metadata;

using System.Threading.Tasks;
using Woohoo.Audio.Core.Cue;
using Woohoo.Audio.Core.IO;

public interface IMetadataProvider
{
    Task<AlbumMetadata?> QueryAsync(CueSheet cueSheet, IMusicContainer container, CancellationToken cancellationToken);
}
