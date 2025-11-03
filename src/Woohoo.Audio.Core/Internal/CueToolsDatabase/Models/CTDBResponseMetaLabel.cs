// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.CueToolsDatabase.Models;

using System;
using System.Xml.Serialization;

[Serializable]
public sealed class CTDBResponseMetaLabel
{
    public CTDBResponseMetaLabel()
    {
        this.Name = string.Empty;
        this.CatNo = string.Empty;
    }

    public CTDBResponseMetaLabel(CTDBResponseMetaLabel src)
    {
        this.Name = src.Name;
        this.CatNo = src.CatNo;
    }

    [XmlAttribute("name")]
    public string Name { get; set; }

    [XmlAttribute("catno")]
    public string CatNo { get; set; }
}
