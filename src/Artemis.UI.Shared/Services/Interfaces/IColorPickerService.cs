using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using Artemis.Core;

namespace Artemis.UI.Shared.Services
{
    internal interface IColorPickerService : IArtemisSharedUIService
    {
        Task<object?> ShowGradientPicker(ColorGradient colorGradient, string dialogHost);

        PluginSetting<bool> PreviewSetting { get; }
        LinkedList<Color> RecentColors { get; }
        PluginSetting<LinkedList<Color>> RecentColorsSetting { get; }
        void StartColorDisplay();
        void StopColorDisplay();
        void UpdateColorDisplay(Color color);
        void QueueRecentColor(Color color);
    }
}