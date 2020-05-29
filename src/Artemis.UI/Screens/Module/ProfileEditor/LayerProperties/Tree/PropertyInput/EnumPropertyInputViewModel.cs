using System;
using System.Collections.Generic;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput.Abstract;
using Artemis.UI.Shared.Utilities;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput
{
    public class EnumPropertyInputViewModel<T> : PropertyInputViewModel<T> where T : Enum
    {
        public EnumPropertyInputViewModel(LayerPropertyViewModel<T> layerPropertyViewModel) : base(layerPropertyViewModel)
        {
            EnumValues = EnumUtilities.GetAllValuesAndDescriptions(typeof(T));
        }
        
        public IEnumerable<ValueDescription> EnumValues { get; }
    }
}