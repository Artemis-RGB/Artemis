using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Models.Profile;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.Module.ProfileEditor.Dialogs;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.ProfileElements.ProfileElement
{
    public abstract class ProfileElementViewModel : PropertyChangedBase
    {
        protected ProfileElementViewModel(ProfileElementViewModel parent, Core.Models.Profile.Abstract.ProfileElement profileElement, ProfileEditorViewModel profileEditorViewModel)
        {
            Parent = parent;
            ProfileElement = profileElement;
            ProfileEditorViewModel = profileEditorViewModel;

            Children = new BindableCollection<ProfileElementViewModel>();
            UpdateProfileElements();
        }

        public abstract bool SupportsChildren { get; }
        public ProfileElementViewModel Parent { get; set; }
        public ProfileEditorViewModel ProfileEditorViewModel { get; set; }

        public Core.Models.Profile.Abstract.ProfileElement ProfileElement { get; set; }
        public BindableCollection<ProfileElementViewModel> Children { get; set; }

        public void SetElementInFront(ProfileElementViewModel source)
        {
            if (source.Parent != Parent)
            {
                source.Parent.RemoveExistingElement(source);
                Parent.AddExistingElement(source);
            }

            Parent.ProfileElement.RemoveChild(source.ProfileElement);
            Parent.ProfileElement.AddChild(source.ProfileElement, ProfileElement.Order);
            Parent.UpdateProfileElements();
        }

        public void SetElementBehind(ProfileElementViewModel source)
        {
            if (source.Parent != Parent)
            {
                source.Parent.RemoveExistingElement(source);
                Parent.AddExistingElement(source);
            }

            Parent.ProfileElement.RemoveChild(source.ProfileElement);
            Parent.ProfileElement.AddChild(source.ProfileElement, ProfileElement.Order + 1);
            Parent.UpdateProfileElements();
        }

        public void RemoveExistingElement(ProfileElementViewModel element)
        {
            if (!SupportsChildren)
                throw new ArtemisUIException("Cannot remove a child from a profile element of type " + ProfileElement.GetType().Name);

            ProfileElement.RemoveChild(element.ProfileElement);
            Children.Remove(element);
            element.Parent = null;
        }

        public void AddExistingElement(ProfileElementViewModel element)
        {
            if (!SupportsChildren)
                throw new ArtemisUIException("Cannot add a child to a profile element of type " + ProfileElement.GetType().Name);

            ProfileElement.AddChild(element.ProfileElement);
            Children.Add(element);
            element.Parent = this;
        }

        public void AddFolder()
        {
            if (!SupportsChildren)
                throw new ArtemisUIException("Cannot add a folder to a profile element of type " + ProfileElement.GetType().Name);

            ProfileElement.AddChild(new Folder(ProfileElement.Profile, ProfileElement, "New folder"));
            UpdateProfileElements();
            ProfileEditorViewModel.OnProfileUpdated();
        }

        public void AddLayer()
        {
            if (!SupportsChildren)
                throw new ArtemisUIException("Cannot add a layer to a profile element of type " + ProfileElement.GetType().Name);

            ProfileElement.AddChild(new Layer(ProfileElement.Profile, ProfileElement, "New layer"));
            UpdateProfileElements();
            ProfileEditorViewModel.OnProfileUpdated();
        }

        // ReSharper disable once UnusedMember.Global - Called from view
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

        // ReSharper disable once UnusedMember.Global - Called from view
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

        private void UpdateProfileElements()
        {
            // Order the children
            var vmsList = Children.OrderBy(v => v.ProfileElement.Order).ToList();
            for (var index = 0; index < vmsList.Count; index++)
            {
                var profileElementViewModel = vmsList[index];
                Children.Move(Children.IndexOf(profileElementViewModel), index);
            }

            // Ensure every child element has an up-to-date VM
            if (ProfileElement.Children != null)
            {
                foreach (var profileElement in ProfileElement.Children.OrderBy(c => c.Order))
                {
                    ProfileElementViewModel existing = null;
                    if (profileElement is Folder folder)
                    {
                        existing = Children.FirstOrDefault(p => p is FolderViewModel vm && vm.ProfileElement == folder);
                        if (existing == null)
                            Children.Add(new FolderViewModel(this, folder, ProfileEditorViewModel));
                    }
                    else if (profileElement is Layer layer)
                    {
                        existing = Children.FirstOrDefault(p => p is LayerViewModel vm && vm.ProfileElement == layer);
                        if (existing == null)
                            Children.Add(new LayerViewModel(this, layer, ProfileEditorViewModel));
                    }

                    existing?.UpdateProfileElements();
                }
            }
        }
    }
}