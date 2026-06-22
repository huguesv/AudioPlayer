// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.LrcDatabase.Web;

using System.Text.Json.Serialization;
using Woohoo.Audio.LrcDatabase.Web.Models;

[JsonSerializable(typeof(LrcLibResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class LrcLibResponseJsonContext : JsonSerializerContext
{
}
