using System.Linq;
using Artemis.Core.Models.Profile.Abstract;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.ProfileElements.Abstract
{
    public abstract class ProfileElementViewModel : PropertyChangedBase
    {
        protected ProfileElementViewModel()
        {
            Children = new BindableCollection<ProfileElementViewModel>();
        }

        public FolderViewModel Parent { get; set; }
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
    }
}