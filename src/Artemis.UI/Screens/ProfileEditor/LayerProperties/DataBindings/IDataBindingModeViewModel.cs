using System;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public interface IDataBindingModeViewModel : IScreen, IDisposable
    {
        bool SupportsTestValue { get; }
        void Update();
        object GetTestValue();
    }
}