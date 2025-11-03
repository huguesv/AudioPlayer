// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.CueToolsDatabase.Models;

using System;
using System.Xml.Serialization;

[Serializable]
public sealed class CTDBResponseMetaRelease
{
    public CTDBResponseMetaRelease()
    {
        this.Date = string.Empty;
        this.Country = string.Empty;
    }

    public CTDBResponseMetaRelease(CTDBResponseMetaRelease src)
    {
        this.Date = src.Date;
        this.Country = src.Country;
    }

    [XmlAttribute("date")]
    public string Date { get; set; }

    [XmlAttribute("country")]
    public string Country { get; set; }
}
