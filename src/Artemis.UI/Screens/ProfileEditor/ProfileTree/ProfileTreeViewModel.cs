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
        private readonly IProfileEditorService _profileEditorService;
        private readonly IProfileTreeVmFactory _profileTreeVmFactory;
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

        public bool CanPasteElement => _profileEditorService.GetCanPasteProfileElement();

        protected override void OnInitialActivate()
        {
            Subscribe();
            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            Unsubscribe();
            base.OnClose();
        }

        private void CreateRootFolderViewModel()
        {
            _updatingTree = true;
            ProfileElement firstChild = _profileEditorService.SelectedProfile?.Children?.FirstOrDefault();
            if (firstChild is not Folder folder)
            {
                ActivateItem(null);
                return;
            }
            
            ActiveItem = _profileTreeVmFactory.FolderViewModel(folder);
            _updatingTree = false;
        }

        #region IDropTarget

        private static DragDropType GetDragDropType(IDropInfo dropInfo)
        {
            if (!(dropInfo.Data is TreeItemViewModel source) || !(dropInfo.TargetItem is TreeItemViewModel target))
                return DragDropType.None;
            if (source == target)
                return DragDropType.None;

            TreeItemViewModel parent = target;
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

        public void DragOver(IDropInfo dropInfo)
        {
            DragDropType dragDropType = GetDragDropType(dropInfo);

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
            if (!(dropInfo.Data is TreeItemViewModel source) || !(dropInfo.TargetItem is TreeItemViewModel target))
                return;
            if (source == target)
                return;

            DragDropType dragDropType = GetDragDropType(dropInfo);
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
            _profileEditorService.SaveSelectedProfileConfiguration();
            Subscribe();
        }

        #endregion

        #region Context menu

        public void AddFolder()
        {
            ActiveItem?.AddFolder();
        }

        public void AddLayer()
        {
            ActiveItem?.AddLayer();
        }

        public void PasteElement()
        {
            Folder rootFolder = _profileEditorService.SelectedProfile?.GetRootFolder();
            if (rootFolder != null)
                _profileEditorService.PasteProfileElement(rootFolder, rootFolder.Children.Count);
        }

        public void ContextMenuOpening(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(nameof(CanPasteElement));
        }

        #endregion

        #region Event handlers

        private void Subscribe()
        {
            _profileEditorService.SelectedProfileChanged += OnSelectedProfileChanged;
            _profileEditorService.SelectedProfileElementChanged += OnSelectedProfileElementChanged;
        }

        private void Unsubscribe()
        {
            _profileEditorService.SelectedProfileChanged -= OnSelectedProfileChanged;
            _profileEditorService.SelectedProfileElementChanged -= OnSelectedProfileElementChanged;
        }

        private void OnSelectedProfileElementChanged(object sender, RenderProfileElementEventArgs e)
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
            {
                SelectedTreeItem = null;
            }
            else
            {
                TreeItemViewModel match = ActiveItem.GetAllChildren().FirstOrDefault(vm => vm.ProfileElement == e.RenderProfileElement);
                if (match != null)
                    SelectedTreeItem = match;
            }
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