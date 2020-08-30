using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Models.Profile;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Exceptions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Dialogs;
using Artemis.UI.Shared.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.TreeItem
{
    public abstract class TreeItemViewModel : PropertyChangedBase, IDisposable
    {
        private readonly IDialogService _dialogService;
        private readonly IFolderVmFactory _folderVmFactory;
        private readonly ILayerVmFactory _layerVmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private readonly IRenderElementService _renderElementService;
        private TreeItemViewModel _parent;
        private ProfileElement _profileElement;

        protected TreeItemViewModel(TreeItemViewModel parent,
            ProfileElement profileElement,
            IProfileEditorService profileEditorService,
            IDialogService dialogService,
            IRenderElementService renderElementService,
            IFolderVmFactory folderVmFactory,
            ILayerVmFactory layerVmFactory)
        {
            _profileEditorService = profileEditorService;
            _dialogService = dialogService;
            _renderElementService = renderElementService;
            _folderVmFactory = folderVmFactory;
            _layerVmFactory = layerVmFactory;

            Parent = parent;
            ProfileElement = profileElement;

            Children = new BindableCollection<TreeItemViewModel>();

            Subscribe();
            UpdateProfileElements();
        }

        public TreeItemViewModel Parent
        {
            get => _parent;
            set => SetAndNotify(ref _parent, value);
        }

        public ProfileElement ProfileElement
        {
            get => _profileElement;
            set => SetAndNotify(ref _profileElement, value);
        }

        public BindableCollection<TreeItemViewModel> Children { get; }

        public abstract bool SupportsChildren { get; }

        public void Dispose()
        {
            Unsubscribe();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public List<TreeItemViewModel> GetAllChildren()
        {
            var children = new List<TreeItemViewModel>();
            foreach (var childFolder in Children)
            {
                // Add all children in this element
                children.Add(childFolder);
                // Add all children of children inside this element
                children.AddRange(childFolder.GetAllChildren());
            }

            return children;
        }

        public void SetElementInFront(TreeItemViewModel source)
        {
            if (source.Parent != Parent)
            {
                source.Parent.RemoveExistingElement(source);
                Parent.AddExistingElement(source);
            }

            Parent.Unsubscribe();
            Parent.ProfileElement.RemoveChild(source.ProfileElement);
            Parent.ProfileElement.AddChild(source.ProfileElement, ProfileElement.Order);
            Parent.Subscribe();

            Parent.UpdateProfileElements();
        }

        public void SetElementBehind(TreeItemViewModel source)
        {
            if (source.Parent != Parent)
            {
                source.Parent.RemoveExistingElement(source);
                Parent.AddExistingElement(source);
            }

            Parent.Unsubscribe();
            Parent.ProfileElement.RemoveChild(source.ProfileElement);
            Parent.ProfileElement.AddChild(source.ProfileElement, ProfileElement.Order + 1);
            Parent.Subscribe();

            Parent.UpdateProfileElements();
        }

        public void RemoveExistingElement(TreeItemViewModel treeItem)
        {
            if (!SupportsChildren)
                throw new ArtemisUIException("Cannot remove a child from a profile element of type " + ProfileElement.GetType().Name);

            ProfileElement.RemoveChild(treeItem.ProfileElement);
            treeItem.Parent = null;
            treeItem.Dispose();
        }

        public void AddExistingElement(TreeItemViewModel treeItem)
        {
            if (!SupportsChildren)
                throw new ArtemisUIException("Cannot add a child to a profile element of type " + ProfileElement.GetType().Name);

            ProfileElement.AddChild(treeItem.ProfileElement);
            treeItem.Parent = this;
        }

        public void AddFolder()
        {
            if (!SupportsChildren)
                throw new ArtemisUIException("Cannot add a folder to a profile element of type " + ProfileElement.GetType().Name);

            ProfileElement.AddChild(new Folder(ProfileElement.Profile, ProfileElement, "New folder"));
            _profileEditorService.UpdateSelectedProfile();
        }

        public void AddLayer()
        {
            if (!SupportsChildren)
                throw new ArtemisUIException("Cannot add a layer to a profile element of type " + ProfileElement.GetType().Name);

            _renderElementService.CreateLayer(ProfileElement.Profile, ProfileElement, "New layer");
            _profileEditorService.UpdateSelectedProfile();
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public async Task RenameElement()
        {
            var result = await _dialogService.ShowDialog<RenameViewModel>(
                new Dictionary<string, object>
                {
                    {"subject", ProfileElement is Folder ? "folder" : "layer"},
                    {"currentName", ProfileElement.Name}
                }
            );
            if (result is string newName)
            {
                ProfileElement.Name = newName;
                _profileEditorService.UpdateSelectedProfile();
            }
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public async Task DeleteElement()
        {
            var result = await _dialogService.ShowConfirmDialog(
                "Delete profile element",
                "Are you sure you want to delete this element? This cannot be undone."
            );

            if (!result)
                return;

            // Farewell, cruel world
            var parent = Parent;
            ProfileElement.Parent?.RemoveChild(ProfileElement);
            parent.RemoveExistingElement(this);

            _profileEditorService.UpdateSelectedProfile();
        }

        public void UpdateProfileElements()
        {
            // Remove VMs that are no longer a child
            var toRemove = Children.Where(c => c.ProfileElement.Parent != ProfileElement).ToList();
            foreach (var treeItemViewModel in toRemove) 
                Children.Remove(treeItemViewModel);

            // Order the children
            var vmsList = Children.OrderBy(v => v.ProfileElement.Order).ToList();
            for (var index = 0; index < vmsList.Count; index++)
            {
                var profileElementViewModel = vmsList[index];
                if (Children.IndexOf(profileElementViewModel) != index)
                    Children.Move(Children.IndexOf(profileElementViewModel), index);
            }

            // Ensure every child element has an up-to-date VM
            if (ProfileElement.Children == null)
                return;

            var newChildren = new List<TreeItemViewModel>();
            foreach (var profileElement in ProfileElement.Children.OrderBy(c => c.Order))
            {
                if (profileElement is Folder folder)
                {
                    if (Children.FirstOrDefault(p => p is FolderViewModel vm && vm.ProfileElement == folder) == null)
                        newChildren.Add(_folderVmFactory.Create((FolderViewModel) this, folder));
                }
                else if (profileElement is Layer layer)
                {
                    if (Children.FirstOrDefault(p => p is LayerViewModel vm && vm.ProfileElement == layer) == null)
                        newChildren.Add(_layerVmFactory.Create((FolderViewModel) this, layer));
                }
            }

            if (!newChildren.Any())
                return;

            // Add the new children in one call, prevent extra UI events
            foreach (var treeItemViewModel in newChildren)
            {
                treeItemViewModel.UpdateProfileElements();
                Children.Add(treeItemViewModel);
            }
        }

        public void EnableToggled()
        {
            _profileEditorService.UpdateSelectedProfile();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        private void Subscribe()
        {
            ProfileElement.ChildAdded += ProfileElementOnChildAdded;
            ProfileElement.ChildRemoved += ProfileElementOnChildRemoved;
        }

        private void Unsubscribe()
        {
            ProfileElement.ChildAdded -= ProfileElementOnChildAdded;
            ProfileElement.ChildRemoved -= ProfileElementOnChildRemoved;
        }

        private void ProfileElementOnChildRemoved(object sender, EventArgs e)
        {
            UpdateProfileElements();
        }

        private void ProfileElementOnChildAdded(object sender, EventArgs e)
        {
            UpdateProfileElements();
        }
    }
}