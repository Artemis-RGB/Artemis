using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Models.Profile.Abstract;
using Artemis.UI.Screens.Module.ProfileEditor.Dialogs;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.ProfileElements.Abstract
{
    public abstract class ProfileElementViewModel : PropertyChangedBase
    {
        protected ProfileElementViewModel(FolderViewModel parent, ProfileElement profileElement, ProfileEditorViewModel profileEditorViewModel)
        {
            Parent = parent;
            ProfileElement = profileElement;
            ProfileEditorViewModel = profileEditorViewModel;

            Children = new BindableCollection<ProfileElementViewModel>();
        }

        public FolderViewModel Parent { get; set; }
        public ProfileEditorViewModel ProfileEditorViewModel { get; set; }

        public ProfileElement ProfileElement { get; set; }
        public BindableCollection<ProfileElementViewModel> Children { get; set; }

        public virtual void UpdateProfileElements()
        {
            // Order the children
            var vmsList = Children.OrderBy(v => v.ProfileElement.Order).ToList();
            for (var index = 0; index < vmsList.Count; index++)
            {
                var profileElementViewModel = vmsList[index];
                Children.Move(Children.IndexOf(profileElementViewModel), index);
            }
        }

        public void SetElementInFront(ProfileElementViewModel source)
        {
            if (source.Parent != Parent)
            {
                source.Parent.RemoveExistingElement(source);
                Parent.AddExistingElement(source);
            }

            Parent.Folder.RemoveChild(source.ProfileElement);
            Parent.Folder.AddChild(source.ProfileElement, ProfileElement.Order);
            Parent.UpdateProfileElements();
        }

        public void SetElementBehind(ProfileElementViewModel source)
        {
            if (source.Parent != Parent)
            {
                source.Parent.RemoveExistingElement(source);
                Parent.AddExistingElement(source);
            }

            Parent.Folder.RemoveChild(source.ProfileElement);
            Parent.Folder.AddChild(source.ProfileElement, ProfileElement.Order + 1);
            Parent.UpdateProfileElements();
        }

        public async Task RenameElement()
        {
            var result = await ProfileEditorViewModel.DialogService.ShowDialog<ProfileElementRenameViewModel>(
                new Dictionary<string, object> {{"profileElement", ProfileElement}}
            );
            if (result is string newName)
            {
                ProfileElement.Name = newName;
                ProfileEditorViewModel.OnProfileUpdated();
            }
        }

        public async Task DeleteElement()
        {
            var result = await ProfileEditorViewModel.DialogService.ShowConfirmDialog(
                "Delete profile element",
                "Are you sure you want to delete this element? This cannot be undone."
            );

            if (!result)
                return;

            // Farewell, cruel world
            var parent = Parent;
            ProfileElement.Parent.RemoveChild(ProfileElement);
            parent.RemoveExistingElement(this);
            parent.UpdateProfileElements();

            ProfileEditorViewModel.OnProfileUpdated();
        }
    }
}