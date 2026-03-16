// Copyright (c) 2009-2025 Gregory S. Chudov.
// Licensed under the LGPL license. See License.txt in the project root for license information.

namespace CUETools.Codecs
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;

    public interface IAudioDecoderSettings
    {
        string Name { get; }

        string Extension { get; }

        Type DecoderType { get; }

        int Priority { get; }

        IAudioDecoderSettings Clone();
    }

    public static class IAudioDecoderSettingsExtensions
    {
        public static bool HasBrowsableAttributes(this IAudioDecoderSettings settings)
        {
            bool hasBrowsable = false;
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(settings))
            {
                bool isBrowsable = true;
                foreach (var attribute in property.Attributes)
                {
                    var browsable = attribute as BrowsableAttribute;
                    isBrowsable &= browsable == null || browsable.Browsable;
                }
                hasBrowsable |= isBrowsable;
            }
            return hasBrowsable;
        }

        public static void Init(this IAudioDecoderSettings settings)
        {
            // Iterate through each property and call ResetValue()
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(settings))
                property.ResetValue(settings);
        }

        public static IAudioSource Open(this IAudioDecoderSettings settings, string path, Stream IO = null)
        {
            return Activator.CreateInstance(settings.DecoderType, settings, path, IO) as IAudioSource;
        }
    }
}
