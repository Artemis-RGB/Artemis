using System.Linq;
using Artemis.Core.Models.Profile;
using Artemis.UI.Screens.Module.ProfileEditor.ProfileElements.Abstract;

namespace Artemis.UI.Screens.Module.ProfileEditor.ProfileElements
{
    public class FolderViewModel : ProfileElementViewModel
    {
        public FolderViewModel(FolderViewModel parent, Folder folder, ProfileEditorViewModel profileEditorViewModel) : base(parent, folder, profileEditorViewModel)
        {
            Folder = folder;
            UpdateProfileElements();
        }

        public Folder Folder { get; }

        public void AddFolder()
        {
            Folder.AddFolder("New folder");
            UpdateProfileElements();

            ProfileEditorViewModel.OnProfileUpdated();
        }

        public void AddLayer()
        {
            Folder.AddLayer("New layer");
            UpdateProfileElements();

            ProfileEditorViewModel.OnProfileUpdated();
        }

        public void RemoveExistingElement(ProfileElementViewModel element)
        {
            Folder.RemoveChild(element.ProfileElement);
            Children.Remove(element);
            element.Parent = null;
        }

        public void AddExistingElement(ProfileElementViewModel element)
        {
            Folder.AddChild(element.ProfileElement);
            Children.Add(element);
            element.Parent = this;
        }

        public sealed override void UpdateProfileElements()
        {
            // Ensure every child element has an up-to-date VM
            if (Folder.Children != null)
            {
                foreach (var profileElement in Folder.Children.OrderBy(c => c.Order))
                {
                    ProfileElementViewModel existing = null;
                    if (profileElement is Folder folder)
                    {
                        existing = Children.FirstOrDefault(p => p is FolderViewModel vm && vm.Folder == folder);
                        if (existing == null)
                            Children.Add(new FolderViewModel(this, folder, ProfileEditorViewModel));
                    }
                    else if (profileElement is Layer layer)
                    {
                        existing = Children.FirstOrDefault(p => p is LayerViewModel vm && vm.Layer == layer);
                        if (existing == null)
                            Children.Add(new LayerViewModel(this, layer, ProfileEditorViewModel));
                    }

                    existing?.UpdateProfileElements();
                }
            }

            base.UpdateProfileElements();
        }
    }
}