using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CSCore.DSP;

namespace Artemis.Profiles.Layers.Types.Audio.AudioCapturing
{
    public class LineSpectrum : SpectrumBase
    {
        private int _barCount;

        public LineSpectrum(FftSize fftSize)
        {
            FftSize = fftSize;
        }

        public int BarCount
        {
            get { return _barCount; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");
                _barCount = value;
                SpectrumResolution = value;
                UpdateFrequencyMapping();
            }
        }

        public List<double> GetLineValues(double height)
        {
            var fftBuffer = new float[(int) FftSize];

            // get the fft result from the spectrum provider
            if (!SpectrumProvider.GetFftData(fftBuffer, this))
                return null;

            var spectrumPoints = CalculateSpectrumPoints(height, fftBuffer);
            return spectrumPoints?.Select(s => s.Value).ToList();

        }
    }
}