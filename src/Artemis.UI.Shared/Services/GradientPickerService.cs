using System.Collections.Generic;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.Colors;
using Artemis.UI.Shared.Screens.GradientEditor;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Shared.Services
{
    public class GradientPickerService : IGradientPickerService
    {
        private readonly IDialogService _dialogService;

        public GradientPickerService(IDialogService dialogService)
        {
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