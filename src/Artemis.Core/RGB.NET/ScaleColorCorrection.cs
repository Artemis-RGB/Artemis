using RGB.NET.Core;

namespace Artemis.Core
{
    internal class ScaleColorCorrection : IColorCorrection
    {
        private readonly ArtemisDevice _device;

        public ScaleColorCorrection(ArtemisDevice device)
        {
            _device = device;
        }

        public void ApplyTo(ref Color color)
        {
            color = color.MultiplyRGB(_device.RedScale, _device.GreenScale, _device.BlueScale);
        }
    }
}