// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.CueToolsDatabase.Models;

using System;
using System.Xml.Serialization;

[Serializable]
public sealed class CTDBResponseMeta
{
    public CTDBResponseMeta()
    {
        this.Source = string.Empty;
        this.Id = string.Empty;
        this.Artist = string.Empty;
        this.Album = string.Empty;
        this.Year = string.Empty;
        this.Genre = string.Empty;
        this.Extra = string.Empty;
        this.DiscNumber = string.Empty;
        this.DiscCount = string.Empty;
        this.DiscName = string.Empty;
        this.InfoUrl = string.Empty;
        this.Barcode = string.Empty;

        this.CoverArts = [];
        this.Tracks = [];
        this.Labels = [];
        this.Releases = [];
    }

    public CTDBResponseMeta(CTDBResponseMeta src)
    {
        this.Source = src.Source;
        this.Id = src.Id;
        this.Artist = src.Artist;
        this.Album = src.Album;
        this.Year = src.Year;
        this.Genre = src.Genre;
        this.Extra = src.Extra;
        this.DiscNumber = src.DiscNumber;
        this.DiscCount = src.DiscCount;
        this.DiscName = src.DiscName;
        this.InfoUrl = src.InfoUrl;
        this.Barcode = src.Barcode;

        if (src.CoverArts != null)
        {
            this.CoverArts = new CTDBResponseMetaImage[src.CoverArts.Length];
            for (var i = 0; i < src.CoverArts.Length; i++)
            {
                this.CoverArts[i] = new CTDBResponseMetaImage(src.CoverArts[i]);
            }
        }
        else
        {
            this.CoverArts = [];
        }

        if (src.Tracks != null)
        {
            this.Tracks = new CTDBResponseMetaTrack[src.Tracks.Length];
            for (var i = 0; i < src.Tracks.Length; i++)
            {
                this.Tracks[i] = new CTDBResponseMetaTrack(src.Tracks[i]);
            }
        }
        else
        {
            this.Tracks = [];
        }

        if (src.Labels != null)
        {
            this.Labels = new CTDBResponseMetaLabel[src.Labels.Length];
            for (var i = 0; i < src.Labels.Length; i++)
            {
                this.Labels[i] = new CTDBResponseMetaLabel(src.Labels[i]);
            }
        }
        else
        {
            this.Labels = [];
        }

        if (src.Releases != null)
        {
            this.Releases = new CTDBResponseMetaRelease[src.Releases.Length];
            for (var i = 0; i < src.Releases.Length; i++)
            {
                this.Releases[i] = new CTDBResponseMetaRelease(src.Releases[i]);
            }
        }
        else
        {
            this.Releases = [];
        }
    }

    [XmlAttribute("source")]
    public string Source { get; set; }

    [XmlAttribute("id")]
    public string Id { get; set; }

    [XmlAttribute("artist")]
    public string Artist { get; set; }

    [XmlAttribute("album")]
    public string Album { get; set; }

    [XmlAttribute("year")]
    public string Year { get; set; }

    [XmlAttribute("genre")]
    public string Genre { get; set; }

    [XmlElement("extra")]
    public string Extra { get; set; }

    [XmlAttribute("discnumber")]
    public string DiscNumber { get; set; }

    [XmlAttribute("disccount")]
    public string DiscCount { get; set; }

    [XmlAttribute("discname")]
    public string DiscName { get; set; }

    [XmlAttribute("infourl")]
    public string InfoUrl { get; set; }

    [XmlAttribute("barcode")]
    public string Barcode { get; set; }

    [XmlElement("coverart")]
    public CTDBResponseMetaImage[]? CoverArts { get; set; }

    [XmlElement("track")]
    public CTDBResponseMetaTrack[]? Tracks { get; set; }

    [XmlElement("label")]
    public CTDBResponseMetaLabel[]? Labels { get; set; }

    [XmlElement("release")]
    public CTDBResponseMetaRelease[]? Releases { get; set; }
}
