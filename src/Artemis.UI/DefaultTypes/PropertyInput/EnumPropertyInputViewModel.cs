using System;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;
using EnumUtilities = Artemis.UI.Shared.EnumUtilities;

namespace Artemis.UI.DefaultTypes.PropertyInput
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