using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactions.DragAndDrop;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.Behaviors;

public class ProfileTreeViewDropHandler : DropHandlerBase
{
    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (e.Source is IControl && sender is TreeView treeView)
            return Validate<TreeItemViewModel>(treeView, e, sourceContext, targetContext, false);

        return false;
    }

    public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        bool result = false;
        if (e.Source is IControl && sender is TreeView treeView)
            result = Validate<TreeItemViewModel>(treeView, e, sourceContext, targetContext, true);

        if (sender is ItemsControl itemsControl)
            foreach (TreeViewItem treeViewItem in GetFlattenedTreeView(itemsControl))
                SetDraggingPseudoClasses(treeViewItem, TreeDropType.None);

        return result;
    }

    public override void Cancel(object? sender, RoutedEventArgs e)
    {
        if (sender is ItemsControl itemsControl)
            foreach (TreeViewItem treeViewItem in GetFlattenedTreeView(itemsControl))
                SetDraggingPseudoClasses(treeViewItem, TreeDropType.None);

        base.Cancel(sender, e);
    }

    private bool Validate<T>(TreeView treeView, DragEventArgs e, object? sourceContext, object? targetContext, bool bExecute) where T : TreeItemViewModel
    {
        Point position = e.GetPosition(treeView);
        IVisual? targetVisual = treeView.GetVisualAt(position).FindAncestorOfType<TreeViewItem>();
        if (sourceContext is not T sourceNode || targetContext is not ProfileTreeViewModel vm || targetVisual is not IControl {DataContext: T targetNode})
            return false;
        if (bExecute && targetNode == sourceNode)
            return false;
        TreeItemViewModel? sourceParent = sourceNode.Parent;
        TreeItemViewModel? targetParent = targetNode.Parent;
        ObservableCollection<TreeItemViewModel> sourceNodes = sourceParent is { } ? sourceParent.Children : vm.Children;
        ObservableCollection<TreeItemViewModel> targetNodes = targetParent is { } ? targetParent.Children : vm.Children;

        int sourceIndex = sourceNodes.IndexOf(sourceNode);
        int targetIndex = targetNodes.IndexOf(targetNode);

        // Update the target index according to the position
        TreeDropType dropType = TreeDropType.Before;
        if (!targetNode.SupportsChildren)
        {
            if (position.Y > targetVisual.Bounds.Top + targetVisual.Bounds.Height / 2)
                dropType = TreeDropType.After;
        }
        else
        {
            IVisual? header = targetVisual.GetVisualDescendants().FirstOrDefault(d => d is Border b && b.Name == "PART_LayoutRoot");
            if (header != null)
            {
                double segments = header.Bounds.Height / 3.0;
                if (position.Y > targetVisual.Bounds.Top + segments * 2)
                    dropType = TreeDropType.After;
                else if (position.Y > targetVisual.Bounds.Top + segments)
                    dropType = TreeDropType.Into;
            }
        }

        if (dropType == TreeDropType.After)
            targetIndex += 1;
        else if (dropType == TreeDropType.Into)
            targetIndex = 0;

        TreeItemViewModel? currentParent = targetNode.Parent;
        while (currentParent != null)
        {
            if (currentParent == sourceNode)
                return false;
            currentParent = currentParent.Parent;
        }

        if (sourceIndex < 0 || targetIndex < 0)
            return false;

        if (e.DragEffects != DragDropEffects.Move)
            return false;

        foreach (TreeViewItem treeViewItem in GetFlattenedTreeView(treeView))
            SetDraggingPseudoClasses(treeViewItem, TreeDropType.None);

        if (bExecute)
        {
            if (dropType == TreeDropType.Into)
            {
                targetNode.InsertElement(sourceNode, 0);
            }
            else
            {
                if (sourceParent == targetParent && sourceIndex < targetIndex)
                    targetIndex--;
                if (targetNode.Parent != null)
                    targetNode.Parent.InsertElement(sourceNode, targetIndex);
            }
        }
        else
        {
            SetDraggingPseudoClasses((IControl) targetVisual, dropType);
        }

        return true;
    }

    private List<TreeViewItem> GetFlattenedTreeView(ItemsControl currentNode)
    {
        List<TreeViewItem> result = new();

        foreach (ItemContainerInfo containerInfo in currentNode.ItemContainerGenerator.Containers)
        {
            if (containerInfo.ContainerControl is TreeViewItem treeViewItem && containerInfo.Item is TreeItemViewModel)
            {
                result.Add(treeViewItem);
                if (treeViewItem.ItemContainerGenerator.Containers.Any())
                    result.AddRange(GetFlattenedTreeView(treeViewItem));
            }
        }

        return result;
    }

    private void SetDraggingPseudoClasses(IControl control, TreeDropType type)
    {
        if (type == TreeDropType.None)
        {
            ((IPseudoClasses) control.Classes).Remove(":dragging");
            ((IPseudoClasses) control.Classes).Remove(":dragging-before");
            ((IPseudoClasses) control.Classes).Remove(":dragging-after");
            ((IPseudoClasses) control.Classes).Remove(":dragging-into");
        }
        else
        {
            ((IPseudoClasses) control.Classes).Add(":dragging");
            if (type == TreeDropType.Before)
            {
                ((IPseudoClasses) control.Classes).Add(":dragging-before");
                ((IPseudoClasses) control.Classes).Remove(":dragging-after");
                ((IPseudoClasses) control.Classes).Remove(":dragging-into");
            }
            else if (type == TreeDropType.After)
            {
                ((IPseudoClasses) control.Classes).Remove(":dragging-before");
                ((IPseudoClasses) control.Classes).Add(":dragging-after");
                ((IPseudoClasses) control.Classes).Remove(":dragging-into");
            }
            else if (type == TreeDropType.Into)
            {
                ((IPseudoClasses) control.Classes).Remove(":dragging-before");
                ((IPseudoClasses) control.Classes).Remove(":dragging-after");
                ((IPseudoClasses) control.Classes).Add(":dragging-into");
            }
        }
    }

    private enum TreeDropType
    {
        Before,
        After,
        Into,
        None
    }
}