// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.
namespace Woohoo.Audio.Core.CueToolsDatabase.Models;

using System;
using System.Xml.Serialization;

[Serializable]
public class CTDBResponseEntry
{
    public CTDBResponseEntry()
    {
        this.Id = 0;
        this.Crc32 = string.Empty;
        this.Confidence = 0;
        this.Npar = 0;
        this.Stride = 0;
        this.HasParity = string.Empty;
        this.Parity = string.Empty;
        this.Syndrome = string.Empty;
        this.TrackCrcs = string.Empty;
        this.Toc = string.Empty;
    }

    [XmlAttribute("id")]
    public long Id { get; set; }

    [XmlAttribute("crc32")]
    public string Crc32 { get; set; }

    [XmlAttribute("confidence")]
    public int Confidence { get; set; }

    [XmlAttribute("npar")]
    public int Npar { get; set; }

    [XmlAttribute("stride")]
    public int Stride { get; set; }

    [XmlAttribute("hasparity")]
    public string HasParity { get; set; }

    [XmlAttribute("parity")]
    public string Parity { get; set; }

    [XmlAttribute("syndrome")]
    public string Syndrome { get; set; }

    [XmlAttribute("trackcrcs")]
    public string TrackCrcs { get; set; }

    [XmlAttribute("toc")]
    public string Toc { get; set; }
}
