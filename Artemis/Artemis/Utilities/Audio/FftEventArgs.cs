using System;
using System.Diagnostics;
using NAudio.Dsp;

namespace Artemis.Utilities.Audio
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