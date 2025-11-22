// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.LrcLibWeb;

using System;
using System.Net;
using System.Text.Json;
using Woohoo.Audio.Core.Internal.LrcLibWeb.Models;

public sealed class LrcLibWebClient
{
    private const string BaseUrl = "https://lrclib.net";

    private static readonly JsonSerializerOptions SerializationOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task<LrcLibResponse?> QueryAsync(string albumTitle, string artistName, string trackTitle, TimeSpan duration, bool allowExternalSources, CancellationToken cancellationToken)
    {
        // https://lrclib.net/docs
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        var userAgent = $"Audio Player v{version} (https://github.com/huguesv/AudioPlayer)";

        var requestUriString = BaseUrl
            + (allowExternalSources ? "/api/get" : "/api/get-cached")
            + "?artist_name=" + EncodeQueryParameter(artistName)
            + "&track_name=" + EncodeQueryParameter(trackTitle)
            + "&album_name=" + EncodeQueryParameter(albumTitle)
            + "&duration=" + ((int)duration.TotalSeconds).ToString();

        var httpClient = new HttpClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUriString);
        request.Headers.UserAgent.ParseAdd(userAgent);

        using var resp = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (resp.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (resp.StatusCode != HttpStatusCode.OK)
        {
            throw new HttpRequestException($"Request failed with status code {resp.StatusCode}");
        }

        using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
        var response = await JsonSerializer.DeserializeAsync<LrcLibResponse>(stream, SerializationOptions, cancellationToken);

        return response;
    }

    private static string EncodeQueryParameter(string value)
    {
        return Uri.EscapeDataString(value); // .Replace("%20", "+");
    }
}
