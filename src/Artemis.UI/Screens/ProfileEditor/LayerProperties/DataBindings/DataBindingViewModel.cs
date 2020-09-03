using System.Linq;
using System.Reflection;
using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings
{
    public class DataBindingViewModel : PropertyChangedBase
    {
        private DataBinding _dataBinding;

        public DataBindingViewModel(BaseLayerProperty layerProperty, PropertyInfo targetProperty)
        {
            LayerProperty = layerProperty;
            TargetProperty = targetProperty;

            DisplayName = TargetProperty.Name.ToUpper();
            ModifierViewModels = new BindableCollection<DataBindingModifierViewModel>();
            DataBinding = layerProperty.DataBindings.FirstOrDefault(d => d.TargetProperty == targetProperty);
        }

        public BaseLayerProperty LayerProperty { get; }
        public PropertyInfo TargetProperty { get; }
        public string DisplayName { get; }
        public BindableCollection<DataBindingModifierViewModel> ModifierViewModels { get; }
        
        public DataBinding DataBinding
        {
            get => _dataBinding;
            set
            {
                if (!SetAndNotify(ref _dataBinding, value)) return;
                UpdateModifierViewModels();
            }
        }

        public void EnableDataBinding()
        {
            if (DataBinding != null)
                return;

            DataBinding = LayerProperty.AddDataBinding(TargetProperty);
        }

        public void RemoveDataBinding()
        {
            if (DataBinding == null)
                return;

            var toRemove = DataBinding;
            DataBinding = null;
            LayerProperty.RemoveDataBinding(toRemove);
        }

        public void AddModifier()
        {
            if (DataBinding == null)
                return;

            var modifier = new DataBindingModifier(ProfileRightSideType.Dynamic);
            DataBinding.AddModifier(modifier);

            ModifierViewModels.Add(new DataBindingModifierViewModel(modifier));
        }

        private void UpdateModifierViewModels()
        {
            ModifierViewModels.Clear();
            if (DataBinding == null)
                return;

            foreach (var dataBindingModifier in DataBinding.Modifiers)
                ModifierViewModels.Add(new DataBindingModifierViewModel(dataBindingModifier));
        }
    }
}