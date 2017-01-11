using System;
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

        public void UpdateLinesVertical(double height, Point[] points)
        {
            var fftBuffer = new float[(int) FftSize];

            // get the fft result from the spectrum provider
            if (!SpectrumProvider.GetFftData(fftBuffer, this))
                return;

            var spectrumPoints = CalculateSpectrumPoints(height, fftBuffer);
            for (var index = 0; index < spectrumPoints.Length; index++)
            {
                var spectrumPointData = spectrumPoints[index];
                points[index].Y = spectrumPointData.Value;
            }
        }

        public void UpdateLinesHorizontal(double width, Point[] points)
        {
            var fftBuffer = new float[(int) FftSize];

            // get the fft result from the spectrum provider
            if (!SpectrumProvider.GetFftData(fftBuffer, this))
                return;

            var spectrumPoints = CalculateSpectrumPoints(width, fftBuffer);
            for (var index = 0; index < spectrumPoints.Length; index++)
            {
                var spectrumPointData = spectrumPoints[index];
                points[index].X = spectrumPointData.Value;
            }
        }
    }
}