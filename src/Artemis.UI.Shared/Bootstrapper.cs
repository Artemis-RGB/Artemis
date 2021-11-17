using Artemis.UI.Shared.Controls;
using Artemis.UI.Shared.Services;
using Ninject;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Represents the main entry point for the shared UI library
    ///     <para>The Artemis UI calls this so there's no need to deal with this in a plugin</para>
    /// </summary>
    public static class Bootstrapper
    {
        /// <summary>
        ///     Gets a boolean indicating whether or not the shared UI library has been initialized
        /// </summary>
        public static bool Initialized { get; private set; }

        /// <summary>
        ///     Initializes the shared UI library
        /// </summary>
        /// <param name="kernel"></param>
        public static void Initialize(IKernel kernel)
        {
            if (Initialized)
                return;

            GradientPicker.ColorPickerService = kernel.Get<IColorPickerService>();
            ColorPicker.ColorPickerService = kernel.Get<IColorPickerService>();
            DataModelPicker.DataModelUIService = kernel.Get<IDataModelUIService>();

            Initialized = true;
        }
    }
}