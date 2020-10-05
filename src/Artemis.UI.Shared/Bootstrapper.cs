using Artemis.UI.Shared.Services;
using Ninject;

namespace Artemis.UI.Shared
{
    public static class Bootstrapper
    {
        public static bool Initialized { get; private set; }

        public static void Initialize(IKernel kernel)
        {
            if (Initialized)
                return;

            IColorPickerService colorPickerService = kernel.Get<IColorPickerService>();
            GradientPicker.ColorPickerService = colorPickerService;
            ColorPicker.ColorPickerService = colorPickerService;

            Initialized = true;
        }
    }
}