using System.Collections.Generic;
using System.Linq;
using Artemis.UI.ViewModels.Interfaces;
using Stylet;

namespace Artemis.UI.ViewModels
{
    public class RootViewModel : Conductor<IArtemisViewModel>.Collection.OneActive
    {
        private readonly ICollection<IArtemisViewModel> _artemisViewModels;

        public RootViewModel(ICollection<IArtemisViewModel> artemisViewModels)
        {
            _artemisViewModels = artemisViewModels;
            // Add the built-in items
            Items.AddRange(artemisViewModels);
            // Activate the home item
            ActiveItem = _artemisViewModels.First(v => v.GetType() == typeof(HomeViewModel));
        }

        public bool MenuOpen { get; set; }

        public void NavigateToHome()
        {
            ActivateItem(_artemisViewModels.First(v => v.GetType() == typeof(HomeViewModel)));
            MenuOpen = false;
        }

        public void NavigateToNews()
        {
        }

        public void NavigateToWorkshop()
        {
        }

        public void NavigateToSettings()
        {
            ActivateItem(_artemisViewModels.First(v => v.GetType() == typeof(SettingsViewModel)));
            MenuOpen = false;
        }
    }
}