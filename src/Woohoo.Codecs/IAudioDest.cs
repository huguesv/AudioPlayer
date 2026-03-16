// Copyright (c) 2009-2025 Gregory S. Chudov.
// Licensed under the LGPL license. See License.txt in the project root for license information.

namespace CUETools.Codecs
{
    public interface IAudioDest
    {
        IAudioEncoderSettings Settings { get; }

        string Path { get; }
        long FinalSampleCount { set; }

        void Write(AudioBuffer buffer);
        void Close();
        void Delete();
    }
}
