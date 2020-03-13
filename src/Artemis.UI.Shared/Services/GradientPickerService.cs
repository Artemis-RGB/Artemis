using Artemis.Core.Models.Profile;
using Artemis.UI.Shared.Ninject.Factories;
using Artemis.UI.Shared.Screens.GradientEditor;
using Artemis.UI.Shared.Services.Interfaces;
using Ninject;
using Stylet;

namespace Artemis.UI.Shared.Services
{
    public class GradientPickerService : IGradientPickerService
    {
        private readonly IGradientEditorVmFactory _gradientEditorVmFactory;
        private readonly IWindowManager _windowManager;

        public GradientPickerService(IGradientEditorVmFactory gradientEditorVmFactory, IWindowManager windowManager)
        {
            _gradientEditorVmFactory = gradientEditorVmFactory;
            _windowManager = windowManager;
        }

        public void ShowGradientPicker(ColorGradient colorGradient)
        {
            _windowManager.ShowDialog(_gradientEditorVmFactory.Create(colorGradient));
        }
    }
}