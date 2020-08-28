using System;
using System.Collections.Generic;
using System.Text;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public class DataBindingsTabsViewModel : PropertyChangedBase
    {
        public DataBindingsTabsViewModel()
        {
            Tabs = new BindableCollection<DataBindingViewModel>();
        }
        public BindableCollection<DataBindingViewModel> Tabs { get; set; }
    }
}
