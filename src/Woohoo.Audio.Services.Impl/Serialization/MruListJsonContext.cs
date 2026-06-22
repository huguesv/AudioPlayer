// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Services.Impl;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

[JsonSerializable(typeof(List<MruItem>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class MruListJsonContext : JsonSerializerContext
{
}
