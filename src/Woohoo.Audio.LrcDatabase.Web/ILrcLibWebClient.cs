// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.LrcDatabase.Web;

using System;
using Woohoo.Audio.LrcDatabase.Web.Models;

public interface ILrcLibWebClient
{
    Task<LrcLibResponse?> QueryAsync(string albumTitle, string artistName, string trackTitle, TimeSpan duration, bool allowExternalSources, CancellationToken cancellationToken);
}
