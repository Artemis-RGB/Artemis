using System;
using System.Collections.Generic;
using System.Text;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Abstract.ViewModels;

namespace Artemis.Plugins.Devices.WS281X.ViewModels
{
    public class WS281XConfigurationViewModel : PluginConfigurationViewModel
    {
        public WS281XConfigurationViewModel(Plugin plugin) : base(plugin)
        {
            var WS281XInstance = RGB.NET.Devices.WS281X.WS281XDeviceProvider.Instance;
            
        }
    }
}
