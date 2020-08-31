using System.Threading.Tasks;
using Artemis.Core;

namespace Artemis.UI.Shared.Services
{
    public interface IGradientPickerService : IArtemisSharedUIService
    {
        Task<object> ShowGradientPicker(ColorGradient colorGradient, string dialogHost);
    }
}