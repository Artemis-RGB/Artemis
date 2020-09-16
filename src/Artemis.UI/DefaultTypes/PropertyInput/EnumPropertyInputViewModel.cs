using System;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.PropertyInput
{
    public class EnumPropertyInputViewModel<T> : PropertyInputViewModel<T> where T : Enum
    {
        public EnumPropertyInputViewModel(LayerProperty<T> layerProperty, IProfileEditorService profileEditorService) : base(layerProperty, profileEditorService)
        {
            EnumValues = new BindableCollection<ValueDescription>(EnumUtilities.GetAllValuesAndDescriptions(typeof(T)));
        }

        public BindableCollection<ValueDescription> EnumValues { get; }
    }
}