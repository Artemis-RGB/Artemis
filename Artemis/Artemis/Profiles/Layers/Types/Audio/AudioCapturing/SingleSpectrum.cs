using System;
using System.Collections.Generic;
using Artemis.Profiles.Layers.Models;
using CSCore.DSP;

namespace Artemis.Profiles.Layers.Types.Audio.AudioCapturing
{
    public sealed class SingleSpectrum : SpectrumBase
    {
        public SingleSpectrum(FftSize fftSize, ISpectrumProvider spectrumProvider)
        {
            SpectrumProvider = spectrumProvider;
            SpectrumResolution = 1;
            FftSize = fftSize;

            UpdateFrequencyMapping();
        }

        public double? GetValue()
        {
            var fftBuffer = new float[(int)FftSize];

            // get the fft result from the spectrum provider
            if (SpectrumProvider == null || !SpectrumProvider.GetFftData(fftBuffer, this))
                return null;

            var spectrumPoints = CalculateSpectrumPoints(1, fftBuffer);
            return spectrumPoints[0].Value;
        }
    }
}