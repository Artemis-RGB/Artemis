using System.Linq;
using Artemis.Core.Models.Profile;
using Artemis.UI.Screens.Module.ProfileEditor.ProfileElements.Abstract;

namespace Artemis.UI.Screens.Module.ProfileEditor.ProfileElements
{
    public class FolderViewModel : ProfileElementViewModel
    {
        public FolderViewModel(Folder folder, FolderViewModel parent)
        {
            Folder = folder;
            Parent = parent;
            ProfileElement = folder;
        }

        public Folder Folder { get; }

        public void AddFolder()
        {
            Folder.AddFolder("New folder");
            UpdateProfileElements();
        }

        public void AddLayer()
        {
            Folder.AddLayer("New layer");
            UpdateProfileElements();
        }

        public void RemoveExistingElement(ProfileElementViewModel element)
        {
            Folder.RemoveChild(element.ProfileElement);
            Children.Remove(element);
            element.Parent = null;

            UpdateProfileElements();
        }

        public void AddExistingElement(ProfileElementViewModel element)
        {
            Folder.AddChild(element.ProfileElement);
            Children.Add(element);
            element.Parent = this;

            UpdateProfileElements();
        }

        public override void UpdateProfileElements()
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
                            Children.Add(new FolderViewModel(folder, this));
                    }
                    else if (profileElement is Layer layer)
                    {
                        existing = Children.FirstOrDefault(p => p is LayerViewModel vm && vm.Layer == layer);
                        if (existing == null)
                            Children.Add(new LayerViewModel(layer, this));
                    }

                    existing?.UpdateProfileElements();
                }
            }

            base.UpdateProfileElements();
        }
    }
}