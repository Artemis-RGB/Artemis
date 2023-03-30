using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactions.DragAndDrop;

namespace Artemis.UI.Screens.Sidebar.Behaviors;

public class SidebarCategoryViewDropHandler : DropHandlerBase
{
    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (sender is ItemsControl itemsControl)
        {
            foreach (Control container in itemsControl.GetRealizedContainers())
                SetDraggingPseudoClasses(container, false, false);
        }

        if (e.Source is Control && sender is ListBox listBox)
            return Validate(listBox, e, sourceContext, targetContext, false);

        return false;
    }

    public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (sender is ItemsControl itemsControl)
        {
            foreach (Control container in itemsControl.GetRealizedContainers())
                SetDraggingPseudoClasses(container, false, false);
        }

        if (e.Source is Control && sender is ListBox listBox)
            return Validate(listBox, e, sourceContext, targetContext, true);

        return false;
    }

    private bool Validate(ListBox listBox, DragEventArgs e, object? sourceContext, object? targetContext, bool bExecute)
    {
        if (sourceContext is not SidebarProfileConfigurationViewModel sourceItem || targetContext is not SidebarCategoryViewModel vm ||
            listBox.GetVisualAt(e.GetPosition(listBox)) is not Control targetControl)
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
                if (positionInTarget.Y > targetVisual.Bounds.Height / 2)
                    before = false;
            }
        }
        
        foreach (Control container in listBox.GetRealizedContainers())
            SetDraggingPseudoClasses(container, false, false);

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

    private void SetDraggingPseudoClasses(Control? control, bool dragging, bool before)
    {
        if (control == null)
            return;
        
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