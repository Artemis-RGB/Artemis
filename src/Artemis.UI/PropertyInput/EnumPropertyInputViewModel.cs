using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.UI.Shared.PropertyInput;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Shared.Utilities;
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