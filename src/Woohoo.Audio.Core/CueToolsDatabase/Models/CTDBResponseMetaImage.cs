// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.CueToolsDatabase.Models;

using System;
using System.Xml.Serialization;

[Serializable]
public class CTDBResponseMetaImage
{
    public CTDBResponseMetaImage()
    {
        this.Uri = string.Empty;
        this.Uri150 = string.Empty;
        this.Height = 0;
        this.Width = 0;
        this.Primary = false;
    }

    public CTDBResponseMetaImage(CTDBResponseMetaImage src)
    {
        this.Uri = src.Uri;
        this.Uri150 = src.Uri150;
        this.Height = src.Height;
        this.Width = src.Width;
        this.Primary = src.Primary;
    }

    [XmlAttribute("uri")]
    public string Uri { get; set; }

    [XmlAttribute("uri150")]
    public string Uri150 { get; set; }

    [XmlAttribute("height")]
    public int Height { get; set; }

    [XmlAttribute("width")]
    public int Width { get; set; }

    [XmlAttribute("primary")]
    public bool Primary { get; set; }
}
