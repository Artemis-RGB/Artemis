using System;
using System.Collections.Generic;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput
{
    public class IntPropertyInputViewModel : PropertyInputViewModel
    {
        public sealed override List<Type> CompatibleTypes { get; } = new List<Type> {typeof(int)};
    }
}