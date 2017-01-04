using System;
using System.Diagnostics;
using NAudio.Dsp;

namespace Artemis.Profiles.Layers.Types.Audio.AudioCapturing
{
    public class FftEventArgs : EventArgs
    {
        [DebuggerStepThrough]
        public FftEventArgs(Complex[] result)
        {
            Result = result;
        }

        public Complex[] Result { get; private set; }
    }
}