namespace CUETools.Codecs.Flake
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;

    public class DecoderSettings : IAudioDecoderSettings
    {
        #region IAudioDecoderSettings implementation
        [Browsable(false)]
        public string Extension => "flac";

        [Browsable(false)]
        public string Name => "cuetools";

        [Browsable(false)]
        public Type DecoderType => typeof(AudioDecoder);

        [Browsable(false)]
        public int Priority => 2;

        public IAudioDecoderSettings Clone()
        {
            return this.MemberwiseClone() as IAudioDecoderSettings;
        }
        #endregion

        public DecoderSettings()
        {
            this.Init();
        }
    }
}
