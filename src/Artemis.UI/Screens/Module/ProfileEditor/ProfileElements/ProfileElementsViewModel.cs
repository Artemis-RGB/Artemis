using System.Linq;
using System.Windows;
using Artemis.Core.Models.Profile;
using Artemis.UI.Screens.Module.ProfileEditor.ProfileElements.Abstract;
using GongSolutions.Wpf.DragDrop;

namespace Artemis.UI.Screens.Module.ProfileEditor.ProfileElements
{
    public class ProfileElementsViewModel : ProfileEditorPanelViewModel, IDropTarget
    {
        public ProfileElementsViewModel()
        {
            CreateRootFolderViewModel();
        }

        public FolderViewModel RootFolder { get; set; }

        public void DragOver(IDropInfo dropInfo)
        {
            var dragDropType = GetDragDropType(dropInfo);

            switch (dragDropType)
            {
                case DragDropType.FolderAdd:
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = DragDropEffects.Move;
                    break;
                case DragDropType.FolderInsertBefore:
                case DragDropType.FolderInsertAfter:
                case DragDropType.LayerInsertBefore:
                case DragDropType.LayerInsertAfter:
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                    dropInfo.Effects = DragDropEffects.Move;
                    break;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            var source = (ProfileElementViewModel) dropInfo.Data;
            var target = (ProfileElementViewModel) dropInfo.TargetItem;

            var dragDropType = GetDragDropType(dropInfo);
            switch (dragDropType)
            {
                case DragDropType.FolderAdd:
                    source.Parent.RemoveExistingElement(source);
                    ((FolderViewModel) target).AddExistingElement(source);
                    break;
                case DragDropType.FolderInsertBefore:
                case DragDropType.LayerInsertBefore:
                    target.SetElementInFront(source);
                    break;
                case DragDropType.FolderInsertAfter:
                case DragDropType.LayerInsertAfter:
                    target.SetElementBehind(source);
                    break;
            }
        }

        public void AddFolder()
        {
            RootFolder?.AddFolder();
        }

        public void AddLayer()
        {
            RootFolder?.AddLayer();
        }

        public override void OnProfileChanged()
        {
            CreateRootFolderViewModel();
            base.OnProfileChanged();
        }

        private void CreateRootFolderViewModel()
        {
            if (!(Profile?.Children?.FirstOrDefault() is Folder folder))
            {
                RootFolder = null;
                return;
            }

            RootFolder = new FolderViewModel(folder, null);
        }

        private DragDropType GetDragDropType(IDropInfo dropInfo)
        {
            var source = dropInfo.Data as ProfileElementViewModel;
            var target = dropInfo.TargetItem as ProfileElementViewModel;
            if (source == target)
                return DragDropType.None;

            if (target is FolderViewModel)
            {
                if (dropInfo.InsertPosition >= RelativeInsertPosition.TargetItemCenter)
                    return DragDropType.FolderAdd;
                if (dropInfo.InsertPosition == RelativeInsertPosition.BeforeTargetItem)
                    return DragDropType.FolderInsertBefore;
                return DragDropType.FolderInsertAfter;
            }

            if (target is LayerViewModel)
            {
                if (dropInfo.InsertPosition == RelativeInsertPosition.BeforeTargetItem)
                    return DragDropType.LayerInsertBefore;
                return DragDropType.LayerInsertAfter;
            }

            return DragDropType.None;
        }
    }

    public enum DragDropType
    {
        None,
        FolderAdd,
        FolderInsertBefore,
        FolderInsertAfter,
        LayerInsertBefore,
        LayerInsertAfter
    }
}