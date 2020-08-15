using System;
using System.Linq;
using System.Windows;
using Artemis.Core.Models.Profile;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.ProfileEditor.ProfileTree.TreeItem;
using Artemis.UI.Shared.Events;
using Artemis.UI.Shared.Services.Interfaces;
using GongSolutions.Wpf.DragDrop;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public class ProfileTreeViewModel : ProfileEditorPanelViewModel, IDropTarget
    {
        private readonly IFolderVmFactory _folderVmFactory;
        private readonly IProfileEditorService _profileEditorService;
        private TreeItemViewModel _selectedTreeItem;
        private bool _updatingTree;
        private FolderViewModel _rootFolder;

        public ProfileTreeViewModel(IProfileEditorService profileEditorService, IFolderVmFactory folderVmFactory)
        {
            _profileEditorService = profileEditorService;
            _folderVmFactory = folderVmFactory;
        }

        public FolderViewModel RootFolder
        {
            get => _rootFolder;
            set => SetAndNotify(ref _rootFolder, value);
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
                    source.Parent.RemoveExistingElement(source);
                    target.AddExistingElement(source);
                    break;
                case DragDropType.InsertBefore:
                    target.SetElementInFront(source);
                    break;
                case DragDropType.InsertAfter:
                    target.SetElementBehind(source);
                    break;
            }

            _profileEditorService.UpdateSelectedProfile();
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void AddFolder()
        {
            RootFolder?.AddFolder();
        }

        // ReSharper disable once UnusedMember.Global - Called from view
        public void AddLayer()
        {
            RootFolder?.AddLayer();
        }

        private void CreateRootFolderViewModel()
        {
            _updatingTree = true;
            var firstChild = _profileEditorService.SelectedProfile?.Children?.FirstOrDefault();
            if (!(firstChild is Folder folder))
            {
                RootFolder = null;
                return;
            }

            RootFolder?.Dispose();
            RootFolder = _folderVmFactory.Create(folder);
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
                parent = parent.Parent;
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

        protected override void OnInitialActivate()
        {
            _profileEditorService.ProfileSelected += OnProfileSelected;
            _profileEditorService.ProfileElementSelected += OnProfileElementSelected;
            CreateRootFolderViewModel();
        }

        protected override void OnClose()
        {
            _profileEditorService.ProfileSelected -= OnProfileSelected;
            _profileEditorService.ProfileElementSelected -= OnProfileElementSelected;

            RootFolder?.Dispose();
            RootFolder = null;
            base.OnClose();
        }

        #region Event handlers

        private void OnProfileElementSelected(object sender, RenderProfileElementEventArgs e)
        {
            if (e.RenderProfileElement == SelectedTreeItem?.ProfileElement)
                return;

            if (RootFolder == null)
            {
                CreateRootFolderViewModel();
                return;
            }

            _updatingTree = true;
            RootFolder.UpdateProfileElements();
            _updatingTree = false;
            if (e.RenderProfileElement == null)
                SelectedTreeItem = null;
            else
            {
                var match = RootFolder.GetAllChildren().FirstOrDefault(vm => vm.ProfileElement == e.RenderProfileElement);
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