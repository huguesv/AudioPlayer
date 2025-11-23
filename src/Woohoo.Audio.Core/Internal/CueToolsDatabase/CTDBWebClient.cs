// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.CueToolsDatabase;

using System;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Woohoo.Audio.Core.Internal.CueToolsDatabase.Models;

internal sealed class CTDBWebClient : ICTDBWebClient
{
    private const string BaseUrl = "http://db.cuetools.net";

    private readonly IHttpClientFactory httpClientFactory;

    public CTDBWebClient(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public Task<CTDBResponse?> QueryAsync(string toc, CancellationToken cancellationToken)
    {
        return this.QueryAsync(toc, ctdb: true, fuzzy: true, CTDBMetadataSearch.Extensive, cancellationToken);
    }

    private async Task<CTDBResponse?> QueryAsync(string toc, bool ctdb, bool fuzzy, CTDBMetadataSearch metadataSearch, CancellationToken cancellationToken)
    {
        var userAgent = "(" + Environment.OSVersion.VersionString + ")";

        var requestUriString = BaseUrl
            + "/lookup2.php"
            + "?version=3"
            + "&ctdb=" + (ctdb ? 1 : 0)
            + "&fuzzy=" + (fuzzy ? 1 : 0)
            + "&metadata=" + (metadataSearch == CTDBMetadataSearch.None ? "none" : metadataSearch == CTDBMetadataSearch.Fast ? "fast" : metadataSearch == CTDBMetadataSearch.Default ? "default" : "extensive")
            + "&toc=" + toc;

        var httpClient = this.httpClientFactory.CreateClient();

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
