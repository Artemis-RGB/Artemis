using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Core.Services;
using Artemis.UI.Exceptions;
using Artemis.UI.Extensions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.Dialogs;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.TreeItem
{
    public abstract class TreeItemViewModel : Conductor<TreeItemViewModel>.Collection.AllActive, IDisposable
    {
        private readonly IDialogService _dialogService;
        private readonly ILayerBrushService _layerBrushService;
        private readonly IProfileEditorService _profileEditorService;
        private readonly IProfileTreeVmFactory _profileTreeVmFactory;
        private readonly IRgbService _rgbService;
        private ProfileElement _profileElement;
        private string _brokenState;

        protected TreeItemViewModel(ProfileElement profileElement,
            IRgbService rgbService,
            IProfileEditorService profileEditorService,
            IDialogService dialogService,
            IProfileTreeVmFactory profileTreeVmFactory,
            ILayerBrushService layerBrushService)
        {
            _rgbService = rgbService;
            _profileEditorService = profileEditorService;
            _dialogService = dialogService;
            _profileTreeVmFactory = profileTreeVmFactory;
            _layerBrushService = layerBrushService;

            ProfileElement = profileElement;

            Subscribe();
            UpdateProfileElements();
        }

        public ProfileElement ProfileElement
        {
            get => _profileElement;
            set => SetAndNotify(ref _profileElement, value);
        }

        public string BrokenState
        {
            get => _brokenState;
            set => SetAndNotify(ref _brokenState, value);
        }

        public bool CanPasteElement => _profileEditorService.GetCanPasteProfileElement();

        public abstract bool SupportsChildren { get; }
        public abstract bool IsExpanded { get; set; }

        public List<TreeItemViewModel> GetAllChildren()
        {
            List<TreeItemViewModel> children = new();
            foreach (TreeItemViewModel childFolder in Items)
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
            TreeItemViewModel sourceParent = (TreeItemViewModel) source.Parent;
            TreeItemViewModel parent = (TreeItemViewModel) Parent;

            // If the parents are different, remove the element from the old parent and add it to the new parent
            if (source.Parent != Parent)
            {
                sourceParent.RemoveExistingElement(source);
                parent.AddExistingElement(source);
            }

            parent.Unsubscribe();
            parent.ProfileElement.RemoveChild(source.ProfileElement);
            parent.ProfileElement.AddChild(source.ProfileElement, ProfileElement.Order);
            parent.Subscribe();

            parent.UpdateProfileElements();
        }

        public void SetElementBehind(TreeItemViewModel source)
        {
            TreeItemViewModel sourceParent = (TreeItemViewModel) source.Parent;
            TreeItemViewModel parent = (TreeItemViewModel) Parent;
            if (source.Parent != Parent)
            {
                sourceParent.RemoveExistingElement(source);
                parent.AddExistingElement(source);
            }

            parent.Unsubscribe();
            parent.ProfileElement.RemoveChild(source.ProfileElement);
            parent.ProfileElement.AddChild(source.ProfileElement, ProfileElement.Order + 1);
            parent.Subscribe();

            parent.UpdateProfileElements();
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

            Folder folder = new(ProfileElement, "New folder");
            ProfileElement.AddChild(folder, 0);
            _profileEditorService.SaveSelectedProfileConfiguration();
        }

        public void AddLayer()
        {
            if (!SupportsChildren)
                throw new ArtemisUIException("Cannot add a layer to a profile element of type " + ProfileElement.GetType().Name);

            Layer layer = new(ProfileElement, "New layer");
            ProfileElement.AddChild(layer, 0);
            // Could be null if the default brush got disabled
            LayerBrushDescriptor brush = _layerBrushService.GetDefaultLayerBrush();
            if (brush != null)
                layer.ChangeLayerBrush(brush);

            layer.AddLeds(_rgbService.EnabledDevices.SelectMany(d => d.Leds));
            _profileEditorService.SaveSelectedProfileConfiguration();
            _profileEditorService.ChangeSelectedProfileElement(layer);
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public async Task RenameElement()
        {
            object result = await _dialogService.ShowDialogAt<RenameViewModel>(
                "ProfileTreeDialog",
                new Dictionary<string, object>
                {
                    {"subject", ProfileElement is Folder ? "folder" : "layer"},
                    {"currentName", ProfileElement.Name}
                }
            );
            if (result is string newName)
            {
                ProfileElement.Name = newName;
                _profileEditorService.SaveSelectedProfileConfiguration();
            }
        }

        public void DeleteElement()
        {
            ProfileElement newSelection = null;
            if (ProfileElement.Parent != null)
            {
                int index = ProfileElement.Parent.Children.IndexOf(ProfileElement);
                // If there is a next element, select that
                if (index < ProfileElement.Parent.Children.Count - 1)
                    newSelection = ProfileElement.Parent.Children[index + 1];
                // Otherwise select the previous element
                else if (index > 0)
                    newSelection = ProfileElement.Parent.Children[index - 1];
                // And if that's not there, fall back to the parent
                else
                    newSelection = ProfileElement.Parent;
            }
            _profileEditorService.ChangeSelectedProfileElement(newSelection as RenderProfileElement);

            // Farewell, cruel world
            TreeItemViewModel parent = (TreeItemViewModel) Parent;
            ProfileElement.Parent?.RemoveChild(ProfileElement);
            parent.RemoveExistingElement(this);

            _profileEditorService.SaveSelectedProfileConfiguration();
        }

        public void DuplicateElement()
        {
            _profileEditorService.DuplicateProfileElement(ProfileElement);
        }

        public void CopyElement()
        {
            _profileEditorService.CopyProfileElement(ProfileElement);
        }

        public void PasteElement()
        {
            if (ProfileElement.Parent is Folder parent)
                _profileEditorService.PasteProfileElement(parent, ProfileElement.Order - 1);
        }

        public void UpdateProfileElements()
        {
            // Remove VMs that are no longer a child
            List<TreeItemViewModel> toRemove = Items.Where(c => c.ProfileElement.Parent != ProfileElement).ToList();
            foreach (TreeItemViewModel treeItemViewModel in toRemove)
                Items.Remove(treeItemViewModel);

            // Add missing children
            List<TreeItemViewModel> newChildren = new();
            foreach (ProfileElement profileElement in ProfileElement.Children.OrderBy(c => c.Order))
                if (profileElement is Folder folder)
                {
                    if (Items.FirstOrDefault(p => p is FolderViewModel vm && vm.ProfileElement == folder) == null)
                        newChildren.Add(_profileTreeVmFactory.FolderViewModel(folder));
                }
                else if (profileElement is Layer layer)
                {
                    if (Items.FirstOrDefault(p => p is LayerViewModel vm && vm.ProfileElement == layer) == null)
                        newChildren.Add(_profileTreeVmFactory.LayerViewModel(layer));
                }

            // Add the new children in one call, prevent extra UI events
            foreach (TreeItemViewModel treeItemViewModel in newChildren)
            {
                treeItemViewModel.UpdateProfileElements();
                Items.Add(treeItemViewModel);
            }

            // Order the children
            ((BindableCollection<TreeItemViewModel>) Items).Sort(i => i.ProfileElement.Order);
        }

        public void SuspendedToggled()
        {
            // If shift is held toggle focus state
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                // Get all profile elements
                List<ProfileElement> elements = ProfileElement.Profile.GetAllFolders().Cast<ProfileElement>().ToList();
                elements.AddRange(ProfileElement.Profile.GetAllLayers().Cast<ProfileElement>().ToList());

                // Separate out the targets of the focus state, the current profile element and all its parents
                List<ProfileElement> targets = ProfileElement.GetAllFolders().Cast<ProfileElement>().ToList();
                targets.AddRange(ProfileElement.GetAllLayers().Cast<ProfileElement>().ToList());
                ProfileElement target = ProfileElement;
                while (target != null)
                {
                    targets.Add(target);
                    target = target.Parent;
                }

                // If any element is suspended, untoggle focus and unsuspend everything
                if (elements.Except(targets).Any(e => e.Suspended))
                {
                    foreach (ProfileElement profileElement in elements)
                        profileElement.Suspended = false;
                }
                // Otherwise suspend everything except the targets
                else
                {
                    foreach (ProfileElement profileElement in elements.Except(targets))
                        profileElement.Suspended = true;
                    foreach (ProfileElement profileElement in targets)
                        profileElement.Suspended = false;
                }
            }

            _profileEditorService.SaveSelectedProfileConfiguration();
        }

        public void ContextMenuOpening(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(nameof(CanPasteElement));
        }

        public abstract void UpdateBrokenState();

        public async Task ShowBrokenStateExceptions()
        {
            List<IBreakableModel> broken = ProfileElement.GetBrokenHierarchy().Where(b => b.BrokenStateException != null).ToList();

            foreach (IBreakableModel current in broken)
            {
                _dialogService.ShowExceptionDialog($"{current.BrokenDisplayName} - {current.BrokenState}", current.BrokenStateException!);
                if (broken.Last() != current)
                {
                    if (!await _dialogService.ShowConfirmDialog("Broken state", "Do you want to view the next exception?"))
                        return;
                }
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

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) Unsubscribe();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}