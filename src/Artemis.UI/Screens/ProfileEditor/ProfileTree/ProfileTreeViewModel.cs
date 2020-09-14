using System;
using System.Linq;
using System.Windows;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.ProfileTree.TreeItem;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using GongSolutions.Wpf.DragDrop;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public class ProfileTreeViewModel : Conductor<FolderViewModel>, IProfileEditorPanelViewModel, IDropTarget
    {
        private readonly IProfileTreeVmFactory _profileTreeVmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private TreeItemViewModel _selectedTreeItem;
        private bool _updatingTree;

        public ProfileTreeViewModel(IProfileEditorService profileEditorService, IProfileTreeVmFactory profileTreeVmFactory)
        {
            _profileEditorService = profileEditorService;
            _profileTreeVmFactory = profileTreeVmFactory;

            CreateRootFolderViewModel();
        }

        public TreeItemViewModel SelectedTreeItem
        {
            get => _selectedTreeItem;
            set
            {
                if (_updatingTree) return;
                if (!SetAndNotify(ref _selectedTreeItem, value)) return;

                if (value != null && value.ProfileElement is RenderProfileElement renderElement)
                    _profileEditorService.ChangeSelectedProfileElement(renderElement);
                else
                    _profileEditorService.ChangeSelectedProfileElement(null);
            }
        }

        public void DragOver(IDropInfo dropInfo)
        {
            var dragDropType = GetDragDropType(dropInfo);

            switch (dragDropType)
            {
                case DragDropType.Add:
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = DragDropEffects.Move;
                    break;
                case DragDropType.InsertBefore:
                case DragDropType.InsertAfter:
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                    dropInfo.Effects = DragDropEffects.Move;
                    break;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            var source = (TreeItemViewModel) dropInfo.Data;
            var target = (TreeItemViewModel) dropInfo.TargetItem;

            var dragDropType = GetDragDropType(dropInfo);
            switch (dragDropType)
            {
                case DragDropType.Add:
                    ((TreeItemViewModel) source.Parent).RemoveExistingElement(source);
                    target.AddExistingElement(source);
                    break;
                case DragDropType.InsertBefore:
                    target.SetElementInFront(source);
                    break;
                case DragDropType.InsertAfter:
                    target.SetElementBehind(source);
                    break;
            }

            Unsubscribe();
            _profileEditorService.UpdateSelectedProfile();
            Subscribe();
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void AddFolder()
        {
            ActiveItem?.AddFolder();
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void AddLayer()
        {
            ActiveItem?.AddLayer();
        }

        protected override void OnActivate()
        {
            Subscribe();
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            Unsubscribe();
            base.OnDeactivate();
        }

        private void CreateRootFolderViewModel()
        {
            _updatingTree = true;
            var firstChild = _profileEditorService.SelectedProfile?.Children?.FirstOrDefault();
            if (!(firstChild is Folder folder))
            {
                ActivateItem(null);
                return;
            }

            ActivateItem(_profileTreeVmFactory.FolderViewModel(folder));
            _updatingTree = false;

            // Auto-select the first layer
            if (_profileEditorService.SelectedProfile != null && SelectedTreeItem == null)
            {
                if (_profileEditorService.SelectedProfile.GetRootFolder().Children.FirstOrDefault() is RenderProfileElement firstElement)
                    Execute.PostToUIThread(() => _profileEditorService.ChangeSelectedProfileElement(firstElement));
            }
        }

        private static DragDropType GetDragDropType(IDropInfo dropInfo)
        {
            var source = (TreeItemViewModel) dropInfo.Data;
            var target = (TreeItemViewModel) dropInfo.TargetItem;
            if (source == target)
                return DragDropType.None;

            var parent = target;
            while (parent != null)
            {
                if (parent == source)
                    return DragDropType.None;
                parent = parent.Parent as TreeItemViewModel;
            }

            switch (dropInfo.InsertPosition)
            {
                case RelativeInsertPosition.AfterTargetItem | RelativeInsertPosition.TargetItemCenter:
                case RelativeInsertPosition.BeforeTargetItem | RelativeInsertPosition.TargetItemCenter:
                    return target.SupportsChildren ? DragDropType.Add : DragDropType.None;
                case RelativeInsertPosition.BeforeTargetItem:
                    return DragDropType.InsertBefore;
                case RelativeInsertPosition.AfterTargetItem:
                    return DragDropType.InsertAfter;
                default:
                    return DragDropType.None;
            }
        }

        #region Event handlers

        private void Subscribe()
        {
            _profileEditorService.ProfileSelected += OnProfileSelected;
            _profileEditorService.ProfileElementSelected += OnProfileElementSelected;
        }

        private void Unsubscribe()
        {
            _profileEditorService.ProfileSelected -= OnProfileSelected;
            _profileEditorService.ProfileElementSelected -= OnProfileElementSelected;
        }

        private void OnProfileElementSelected(object sender, RenderProfileElementEventArgs e)
        {
            if (e.RenderProfileElement == SelectedTreeItem?.ProfileElement)
                return;

            if (ActiveItem == null)
            {
                CreateRootFolderViewModel();
                return;
            }

            _updatingTree = true;
            ActiveItem.UpdateProfileElements();
            _updatingTree = false;
            if (e.RenderProfileElement == null)
                SelectedTreeItem = null;
            else
            {
                var match = ActiveItem.GetAllChildren().FirstOrDefault(vm => vm.ProfileElement == e.RenderProfileElement);
                if (match != null)
                    SelectedTreeItem = match;
            }
        }

        private void OnProfileSelected(object sender, EventArgs e)
        {
            CreateRootFolderViewModel();
        }

        #endregion
    }

    public enum DragDropType
    {
        None,
        Add,
        InsertBefore,
        InsertAfter
    }
}