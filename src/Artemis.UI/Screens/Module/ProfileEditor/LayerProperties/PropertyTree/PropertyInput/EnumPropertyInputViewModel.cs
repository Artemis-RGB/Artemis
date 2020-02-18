using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Utilities;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput
{
    public class EnumPropertyInputViewModel : PropertyInputViewModel
    {
        public EnumPropertyInputViewModel(IProfileEditorService profileEditorService) : base(profileEditorService)
        {
        }

        public IEnumerable<ValueDescription> EnumValues { get; private set; }

        public sealed override List<Type> CompatibleTypes { get; } = new List<Type> {typeof(Enum)};

        public object EnumInputValue
        {
            get => InputValue ?? Enum.GetValues(LayerPropertyViewModel.LayerProperty.Type).Cast<object>().First();
            set => InputValue = value;
        }

        public override void Update()
        {
            NotifyOfPropertyChange(() => EnumInputValue);
        }

        public override void ApplyInputDrag(object startValue, double dragDistance)
        {
            throw new NotImplementedException();
        }

        protected override void OnInitialized()
        {
            EnumValues = EnumUtilities.GetAllValuesAndDescriptions(LayerPropertyViewModel.LayerProperty.Type);
        }
    }
}