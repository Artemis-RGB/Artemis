using System.Collections.Generic;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings.DirectDataBinding.ModifierTypes
{
    public class ModifierTypeCategoryViewModel : IModifierTypeViewModel
    {
        public ModifierTypeCategoryViewModel(string category, IEnumerable<IModifierTypeViewModel> children)
        {
            Category = category;
            Children = children == null
                ? new BindableCollection<IModifierTypeViewModel>()
                : new BindableCollection<IModifierTypeViewModel>(children);
        }

        public string Category { get; set; }
        public BindableCollection<IModifierTypeViewModel> Children { get; set; }
    }
}