using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Artemis.Profiles.Layers.Types.AngularBrush.Drawing
{
    public class GradientDrawer
    {
        #region Constants

        private static readonly double ORIGIN = Math.Atan2(-1, 0);

        #endregion  

        #region Properties & Fields

        private WriteableBitmap _bitmap;

        private IList<Tuple<double, Color>> _gradientStops;
        public IList<Tuple<double, Color>> GradientStops
        {
            set { _gradientStops = FixGradientStops(value); }
        }

        public Brush Brush { get; private set; }

        #endregion

        #region Methods

        public void Update()
        {
            if (_bitmap == null)
                CreateBrush();

            unsafe
            {
                _bitmap.Lock();
                byte* buffer = (byte*)_bitmap.BackBuffer.ToPointer();

                int width = _bitmap.PixelWidth;
                double widthHalf = width / 2.0;

                int height = _bitmap.PixelHeight;
                double heightHalf = height / 2.0;

                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        int offset = (((y * width) + x) * 4);

                        double gradientOffset = CalculateGradientOffset(x, y, widthHalf, heightHalf);
                        GetColor(_gradientStops, gradientOffset,
                            ref buffer[offset + 3], ref buffer[offset + 2],
                            ref buffer[offset + 1], ref buffer[offset]);
                    }

                _bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
                _bitmap.Unlock();
            }
        }

        private void CreateBrush()
        {
            _bitmap = new WriteableBitmap(10, 10, 96, 96, PixelFormats.Bgra32, null);
            Brush = new ImageBrush(_bitmap) { Stretch = Stretch.UniformToFill };
        }

        private double CalculateGradientOffset(double x, double y, double centerX, double centerY)
        {
            double angle = Math.Atan2(y - centerY, x - centerX) - ORIGIN;
            if (angle < 0) angle += Math.PI * 2;
            return angle / (Math.PI * 2);
        }

        private static void GetColor(IList<Tuple<double, Color>> gradientStops, double offset, ref byte colA, ref byte colR, ref byte colG, ref byte colB)
        {
            if (gradientStops.Count == 0)
            {
                colA = 0;
                colR = 0;
                colG = 0;
                colB = 0;
                return;
            }
            if (gradientStops.Count == 1)
            {
                Color color = gradientStops.First().Item2;
                colA = color.A;
                colR = color.R;
                colG = color.G;
                colB = color.B;
                return;
            }

            Tuple<double, Color> beforeStop = null;
            double afterOffset = -1;
            Color afterColor = default(Color);

            for (int i = 0; i < gradientStops.Count; i++)
            {
                Tuple<double, Color> gradientStop = gradientStops[i];
                double o = gradientStop.Item1;
                if (o <= offset)
                    beforeStop = gradientStop;

                if (o >= offset)
                {
                    afterOffset = gradientStop.Item1;
                    afterColor = gradientStop.Item2;
                    break;
                }
            }
            double beforeOffset = beforeStop.Item1;
            Color beforeColor = beforeStop.Item2;

            double blendFactor = 0f;
            if (beforeOffset != afterOffset)
                blendFactor = ((offset - beforeOffset) / (afterOffset - beforeOffset));

            colA = (byte)((afterColor.A - beforeColor.A) * blendFactor + beforeColor.A);
            colR = (byte)((afterColor.R - beforeColor.R) * blendFactor + beforeColor.R);
            colG = (byte)((afterColor.G - beforeColor.G) * blendFactor + beforeColor.G);
            colB = (byte)((afterColor.B - beforeColor.B) * blendFactor + beforeColor.B);
        }

        private IList<Tuple<double, Color>> FixGradientStops(IList<Tuple<double, Color>> gradientStops)
        {
            if (gradientStops == null) return new List<Tuple<double, Color>>();

            List<Tuple<double, Color>> stops = gradientStops.OrderBy(x => x.Item1).ToList();

            Tuple<double, Color> firstStop = stops.First();
            if (firstStop.Item1 > 0)
                stops.Insert(0, new Tuple<double, Color>(0, firstStop.Item2));

            Tuple<double, Color> lastStop = stops.Last();
            if (lastStop.Item1 < 1)
                stops.Add(new Tuple<double, Color>(1, lastStop.Item2));

            return stops;
        }

        #endregion
    }
}
