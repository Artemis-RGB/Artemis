using System.Collections.Generic;
using Artemis.Core.Models.Profile;
using Artemis.UI.Shared.Ninject.Factories;
using Artemis.UI.Shared.Screens.GradientEditor;
using Artemis.UI.Shared.Services.Interfaces;
using LiteDB.Engine;
using Ninject;
using Stylet;

namespace Artemis.UI.Shared.Services
{
    public class GradientPickerService : IGradientPickerService
    {
        private readonly IGradientEditorVmFactory _gradientEditorVmFactory;
        private readonly IDialogService _dialogService;
        private readonly IWindowManager _windowManager;

        public GradientPickerService(IGradientEditorVmFactory gradientEditorVmFactory, IDialogService dialogService)
        {
            _gradientEditorVmFactory = gradientEditorVmFactory;
            _dialogService = dialogService;
        }

        public void ShowGradientPicker(ColorGradient colorGradient, string dialogHost)
        {
            if (!string.IsNullOrWhiteSpace(dialogHost))
                _dialogService.ShowDialogAt<GradientEditorViewModel>(dialogHost, new Dictionary<string, object> {{"colorGradient", colorGradient}});
            else
                _dialogService.ShowDialog<GradientEditorViewModel>(new Dictionary<string, object> {{"colorGradient", colorGradient}});
        }
    }
}