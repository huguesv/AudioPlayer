// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.CueToolsDatabase;

using System;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Woohoo.Audio.Core.CueToolsDatabase.Models;

public sealed class CTDBClient
{
    private const string DefaultServer = "db.cuetools.net";

    public async Task<CTDBResponse?> QueryAsync(string toc, bool ctdb, bool fuzzy, CTDBMetadataSearch metadataSearch, CancellationToken cancellationToken)
    {
        string userAgent = "(" + Environment.OSVersion.VersionString + ")";
        string urlbase = $"http://{DefaultServer}";

        var requestUriString = urlbase
            + "/lookup2.php"
            + "?version=3"
            + "&ctdb=" + (ctdb ? 1 : 0)
            + "&fuzzy=" + (fuzzy ? 1 : 0)
            + "&metadata=" + (metadataSearch == CTDBMetadataSearch.None ? "none" : metadataSearch == CTDBMetadataSearch.Fast ? "fast" : metadataSearch == CTDBMetadataSearch.Default ? "default" : "extensive")
            + "&toc=" + toc;

        var httpClient = new HttpClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUriString);
        request.Headers.UserAgent.ParseAdd(userAgent);

        using var resp = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (resp.StatusCode != HttpStatusCode.OK)
        {
            throw new HttpRequestException($"Request failed with status code {resp.StatusCode}");
        }

        var serializer = new XmlSerializer(typeof(CTDBResponse));

        using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
        var response = serializer.Deserialize(stream) as CTDBResponse;
        return response;
    }
}
