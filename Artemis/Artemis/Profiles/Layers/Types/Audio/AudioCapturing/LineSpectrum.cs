using System;
using System.Collections.Generic;
using Artemis.Profiles.Layers.Models;
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

        public void SetupLayersVertical(double height, List<LayerModel> audioLayers)
        {
            var fftBuffer = new float[(int)FftSize];

            // get the fft result from the spectrum provider
            if (!SpectrumProvider.GetFftData(fftBuffer, this))
                return;

            var spectrumPoints = CalculateSpectrumPoints(height, fftBuffer);
            foreach (var p in spectrumPoints)
                audioLayers[p.SpectrumPointIndex].Height = p.Value;
        }

        public void SetupLayersHorizontal(double width, List<LayerModel> audioLayers)
        {
            var fftBuffer = new float[(int)FftSize];

            // get the fft result from the spectrum provider
            if (!SpectrumProvider.GetFftData(fftBuffer, this))
                return;

            var spectrumPoints = CalculateSpectrumPoints(width, fftBuffer);
            foreach (var p in spectrumPoints)
                audioLayers[p.SpectrumPointIndex].Width = p.Value;
        }
    }
}