// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.CueToolsDatabase.Models;

using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ctdb", Namespace = "http://db.cuetools.net/ns/mmd-1.0#")]
public class CTDBResponse
{
    public CTDBResponse()
    {
        this.Status = string.Empty;
        this.UpdateUrl = string.Empty;
        this.UpdateMsg = string.Empty;
        this.Message = string.Empty;
        this.Npar = 0;
        this.Entries = [];
        this.Metadatas = [];
    }

    [XmlIgnore]
    public bool ParityNeeded
    {
        get
        {
            return this.Status == "parity needed";
        }
    }

    [XmlAttribute("status")]
    public string Status { get; set; }

    [XmlAttribute("updateurl")]
    public string UpdateUrl { get; set; }

    [XmlAttribute("updatemsg")]
    public string UpdateMsg { get; set; }

    [XmlAttribute("message")]
    public string Message { get; set; }

    [XmlAttribute("npar")]
    public int Npar { get; set; }

    [XmlElement("entry")]
    public CTDBResponseEntry[]? Entries { get; set; }

    [XmlElement("metadata")]
    public CTDBResponseMeta[]? Metadatas { get; set; }
}
