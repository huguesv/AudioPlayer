// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.CueToolsDatabase.Web;

using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using Woohoo.Audio.CueToolsDatabase.Web.Models;

[UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode", Justification = "Types referenced in CTDBResponse.")]
internal static class CTDBSerialization
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CTDBResponse))]
    public static CTDBResponse? Deserialize(Stream stream)
    {
        var serializer = new XmlSerializer(typeof(CTDBResponse));
        var response = serializer.Deserialize(stream) as CTDBResponse;
        return response;
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CTDBResponse))]
    public static void Serialize(Stream stream, CTDBResponse response)
    {
        var serializer = new XmlSerializer(typeof(CTDBResponse));
        serializer.Serialize(stream, response);
    }
}
