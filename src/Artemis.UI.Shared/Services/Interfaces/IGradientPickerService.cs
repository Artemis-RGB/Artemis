using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.Colors;

namespace Artemis.UI.Shared.Services.Interfaces
{
    public interface IGradientPickerService : IArtemisSharedUIService
    {
        void ShowGradientPicker(ColorGradient colorGradient, string dialogHost);
    }
}