// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Metadata;

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Woohoo.Audio.Core.Cue;
using Woohoo.Audio.Core.CueToolsDatabase;
using Woohoo.Audio.Core.CueToolsDatabase.Models;
using Woohoo.Audio.Core.IO;

public sealed class MetadataProvider : IMetadataProvider
{
    private readonly CTDBCachingClient databaseClient = new(Path.Combine(Path.GetTempPath(), "Woohoo.Audio", "CTDBCache"));

    public async Task<AlbumMetadata?> QueryAsync(CueSheet cueSheet, IMusicContainer container, CancellationToken cancellationToken)
    {
        try
        {
            var response = await this.databaseClient.QueryAsync(
                CTDBTocCalculator.GetTocFromCue(cueSheet, container),
                cancellationToken: CancellationToken.None);

            if (response is null)
            {
                return null;
            }

            var bestTitleMetadata = GetBestTitleMetadata(response);
            var bestArtMetadata = GetBestArtMetadata(response);

            if (bestTitleMetadata is null && bestArtMetadata is null)
            {
                return null;
            }

            return new AlbumMetadata
            {
                Album = bestTitleMetadata?.Album ?? string.Empty,
                Artist = bestTitleMetadata?.Artist ?? string.Empty,
                Tracks = bestTitleMetadata?.Tracks?.Select(t => new TrackMetadata
                {
                    Name = t.Name ?? string.Empty,
                    Artist = t.Artist ?? string.Empty,
                }).ToImmutableArray() ?? [],
                Images = bestArtMetadata?.CoverArts?.Select(img => new ArtMetadata
                {
                    IsPrimary = img.Primary,
                    Url = img.Uri ?? string.Empty,
                    SmallUrl = img.Uri150 ?? string.Empty,
                }).ToImmutableArray() ?? [],
            };
        }
        catch
        {
            return null;
        }
    }

    private static CTDBResponseMeta? GetBestTitleMetadata(CTDBResponse response)
    {
        return response.Metadatas?.FirstOrDefault(m => m.Tracks?.Length > 0);
    }

    private static CTDBResponseMeta? GetBestArtMetadata(CTDBResponse response)
    {
        return response.Metadatas?.FirstOrDefault(m => m.Tracks?.Length > 0 && m.CoverArts?.Length > 0 && (m.CoverArts?.Any(a => a.Primary) ?? false));
    }
}
