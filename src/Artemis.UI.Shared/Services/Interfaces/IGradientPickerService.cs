using System.Threading.Tasks;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.Colors;

namespace Artemis.UI.Shared.Services.Interfaces
{
    public interface IGradientPickerService : IArtemisSharedUIService
    {
        Task<object> ShowGradientPicker(ColorGradient colorGradient, string dialogHost);
    }
}