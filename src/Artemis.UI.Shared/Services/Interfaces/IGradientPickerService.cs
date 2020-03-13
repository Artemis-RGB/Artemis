using Artemis.Core.Models.Profile;

namespace Artemis.UI.Shared.Services.Interfaces
{
    public interface IGradientPickerService : IArtemisSharedUIService
    {
        void ShowGradientPicker(ColorGradient colorGradient);
    }
}