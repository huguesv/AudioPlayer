// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.LrcLibWeb;

using System.Text.Json.Serialization;
using Woohoo.Audio.Core.Internal.LrcLibWeb.Models;

[JsonSerializable(typeof(LrcLibResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class LrcLibResponseJsonContext : JsonSerializerContext
{
}
