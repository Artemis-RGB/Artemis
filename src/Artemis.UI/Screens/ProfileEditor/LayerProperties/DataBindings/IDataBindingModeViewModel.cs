using System;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public interface IDataBindingModeViewModel : IScreen, IDisposable
    {
        void Update();
        bool SupportsTestValue { get; }
        object GetTestValue();
    }
}