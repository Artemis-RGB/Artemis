using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGB.NET.Core;

namespace Artemis.UI.Extensions
{
    public static class RgbRectangleExtensions
    {
        public static System.Drawing.Rectangle ToDrawingRectangle(this Rectangle rectangle)
        {
            return new System.Drawing.Rectangle((int) rectangle.X, (int) rectangle.Y, (int) rectangle.Width, (int) rectangle.Height);
        }
    }
}