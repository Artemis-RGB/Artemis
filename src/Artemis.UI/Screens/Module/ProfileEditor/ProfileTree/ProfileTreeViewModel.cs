using System;
using System.Linq;
using System.Windows;
using Artemis.Core.Models.Profile;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Module.ProfileEditor.ProfileTree.TreeItem;
using Artemis.UI.Services.Interfaces;
using GongSolutions.Wpf.DragDrop;

namespace Artemis.UI.Screens.Module.ProfileEditor.ProfileTree
{
    public class ProfileTreeViewModel : ProfileEditorPanelViewModel, IDropTarget
    {
        private readonly IProfileEditorService _profileEditorService;
        private readonly IFolderViewModelFactory _folderViewModelFactory;
        private TreeItemViewModel _selectedTreeItem;

        public ProfileTreeViewModel(IProfileEditorService profileEditorService, IFolderViewModelFactory folderViewModelFactory)
        {
            _profileEditorService = profileEditorService;
            _folderViewModelFactory = folderViewModelFactory;

            CreateRootFolderViewModel();
            _profileEditorService.SelectedProfileChanged += OnSelectedProfileChanged;
            _profileEditorService.SelectedProfileElementChanged += OnSelectedElementChanged;
        }

        public FolderViewModel RootFolder { get; set; }

        public TreeItemViewModel SelectedTreeItem
        {
            get => _selectedTreeItem;
            set
            {
                _selectedTreeItem = value;
                _profileEditorService.ChangeSelectedProfileElement(value?.ProfileElement);
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
            var firstChild = _profileEditorService.SelectedProfile?.Children?.FirstOrDefault();
            if (!(firstChild is Folder folder))
            {
                RootFolder = null;
                return;
            }

            RootFolder = _folderViewModelFactory.Create(folder);
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

        #region Event handlers

        private void OnSelectedElementChanged(object sender, EventArgs e)
        {
            var vms = RootFolder?.GetAllChildren();

            // Don't set it using the setter or that will trigger the event again
            _selectedTreeItem = vms?.FirstOrDefault(vm => vm.ProfileElement == _profileEditorService.SelectedProfileElement);
            NotifyOfPropertyChange(() => SelectedTreeItem);
        }

        private void OnSelectedProfileChanged(object sender, EventArgs e)
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