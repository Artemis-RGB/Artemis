using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Screens.ProfileEditor.ProfileTree;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactions.DragAndDrop;

namespace Artemis.UI.Screens.Sidebar.Behaviors;

public class SidebarCategoryViewDropHandler : DropHandlerBase
{
    private bool Validate(ListBox listBox, DragEventArgs e, object? sourceContext, object? targetContext, bool bExecute)
    {
        if (sourceContext is not SidebarProfileConfigurationViewModel sourceItem || targetContext is not SidebarCategoryViewModel vm ||
            listBox.GetVisualAt(e.GetPosition(listBox)) is not IControl targetControl)
            return false;
        if (e.DragEffects != DragDropEffects.Move)
            return false;

        SidebarProfileConfigurationViewModel? targetItem = targetControl.DataContext as SidebarProfileConfigurationViewModel;
        ListBoxItem? targetVisual = null;
        bool before = true;
        if (targetItem != null)
        {
            Point position = e.GetPosition(listBox);
            targetVisual = listBox.GetVisualAt(position).FindAncestorOfType<ListBoxItem>();
            if (targetVisual != null)
            {
                Point positionInTarget = e.GetPosition(targetVisual);
                if (positionInTarget.Y > (targetVisual.Bounds.Height / 2))
                    before = false;
            }
        }

        foreach (ItemContainerInfo? item in listBox.ItemContainerGenerator.Containers)
            SetDraggingPseudoClasses(item.ContainerControl, false, false);

        if (bExecute)
        {
            if (targetItem != null)
            {
                int index = vm.ProfileConfigurations.IndexOf(targetItem);
                if (!before)
                    index++;
                vm.AddProfileConfiguration(sourceItem.ProfileConfiguration, index);
            }
            else
            {
                vm.AddProfileConfiguration(sourceItem.ProfileConfiguration, null);
            }
        }
        else if (targetVisual != null)
        {
            SetDraggingPseudoClasses(targetVisual, true, before);
        }

        return true;
    }

    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (sender is ItemsControl itemsControl)
        {
            foreach (ItemContainerInfo? item in itemsControl.ItemContainerGenerator.Containers)
                SetDraggingPseudoClasses(item.ContainerControl, false, false);
        }

        if (e.Source is IControl && sender is ListBox listBox)
        {
            return Validate(listBox, e, sourceContext, targetContext, false);
        }

        return false;
    }

    public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (sender is ItemsControl itemsControl)
        {
            foreach (ItemContainerInfo? item in itemsControl.ItemContainerGenerator.Containers)
                SetDraggingPseudoClasses(item.ContainerControl, false, false);
        }

        if (e.Source is IControl && sender is ListBox listBox)
        {
            return Validate(listBox, e, sourceContext, targetContext, true);
        }

        return false;
    }

    private void SetDraggingPseudoClasses(IControl control, bool dragging, bool before)
    {
        if (!dragging)
        {
            ((IPseudoClasses) control.Classes).Remove(":dragging");
            ((IPseudoClasses) control.Classes).Remove(":dragging-before");
            ((IPseudoClasses) control.Classes).Remove(":dragging-after");
            ((IPseudoClasses) control.Classes).Remove(":dragging-into");
        }
        else
        {
            ((IPseudoClasses) control.Classes).Add(":dragging");
            if (before)
            {
                ((IPseudoClasses) control.Classes).Add(":dragging-before");
                ((IPseudoClasses) control.Classes).Remove(":dragging-after");
                ((IPseudoClasses) control.Classes).Remove(":dragging-into");
            }
            else
            {
                ((IPseudoClasses) control.Classes).Remove(":dragging-before");
                ((IPseudoClasses) control.Classes).Add(":dragging-after");
                ((IPseudoClasses) control.Classes).Remove(":dragging-into");
            }
        }
    }
}