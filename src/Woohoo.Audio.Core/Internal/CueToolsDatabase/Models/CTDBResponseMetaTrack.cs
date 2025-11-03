// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.CueToolsDatabase.Models;

using System;
using System.Xml.Serialization;

[Serializable]
public sealed class CTDBResponseMetaTrack
{
    public CTDBResponseMetaTrack()
    {
        this.Name = string.Empty;
        this.Artist = string.Empty;
        this.Extra = string.Empty;
    }

    public CTDBResponseMetaTrack(CTDBResponseMetaTrack src)
    {
        this.Name = src.Name;
        this.Artist = src.Artist;
        this.Extra = src.Extra;
    }

    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlAttribute("artist")]
    public string Artist { get; set; }

    [XmlElement("extra")]
    public string Extra { get; set; }
}
