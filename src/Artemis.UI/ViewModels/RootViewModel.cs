using System.Collections.Generic;
using System.Linq;
using Artemis.UI.ViewModels.Interfaces;
using Stylet;

namespace Artemis.UI.ViewModels
{
    public class RootViewModel : Conductor<IArtemisViewModel>.Collection.OneActive
    {
        public RootViewModel(ICollection<IArtemisViewModel> artemisViewModels)
        {
            // Add the built-in items
            Items.AddRange(artemisViewModels);
            // Activate the home item
            ActiveItem = artemisViewModels.First(v => v.GetType() == typeof(HomeViewModel));
        }
    }
}