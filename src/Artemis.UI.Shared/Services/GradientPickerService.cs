using System.Collections.Generic;
using System.Threading.Tasks;
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

        public Task<object> ShowGradientPicker(ColorGradient colorGradient, string dialogHost)
        {
            if (!string.IsNullOrWhiteSpace(dialogHost))
                return _dialogService.ShowDialogAt<GradientEditorViewModel>(dialogHost, new Dictionary<string, object> {{"colorGradient", colorGradient}});
            return _dialogService.ShowDialog<GradientEditorViewModel>(new Dictionary<string, object> {{"colorGradient", colorGradient}});
        }
    }
}