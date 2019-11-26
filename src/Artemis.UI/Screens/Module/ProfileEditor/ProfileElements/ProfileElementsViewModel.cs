using System.Linq;
using System.Windows;
using Artemis.Core.Models.Profile;
using Artemis.UI.Screens.Module.ProfileEditor.ProfileElements.ProfileElement;
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
            var source = (ProfileElementViewModel) dropInfo.Data;
            var target = (ProfileElementViewModel) dropInfo.TargetItem;

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

            ProfileEditorViewModel.OnProfileUpdated();
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

        public override void ActiveProfileChanged()
        {
            CreateRootFolderViewModel();
            base.ActiveProfileChanged();
        }

        private void CreateRootFolderViewModel()
        {
            if (!(ProfileEditorViewModel?.SelectedProfile?.Children?.FirstOrDefault() is Folder folder))
            {
                RootFolder = null;
                return;
            }

            RootFolder = new FolderViewModel(null, folder, ProfileEditorViewModel);
        }

        private static DragDropType GetDragDropType(IDropInfo dropInfo)
        {
            var source = (ProfileElementViewModel) dropInfo.Data;
            var target = (ProfileElementViewModel) dropInfo.TargetItem;
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
    }

    public enum DragDropType
    {
        None,
        Add,
        InsertBefore,
        InsertAfter
    }
}