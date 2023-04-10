using System;
using System.Reactive.Disposables;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree;

public partial class FolderTreeItemView : ReactiveUserControl<FolderTreeItemViewModel>
{
    public FolderTreeItemView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            ViewModel?.Rename.Subscribe(_ =>
            {
                Input.Focus();
                Input.SelectAll();
            }).DisposeWith(d);
        });
    }


    private void InputElement_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            ViewModel?.SubmitRename();
        else if (e.Key == Key.Escape)
            ViewModel?.CancelRename();
    }

    private void InputElement_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        ViewModel?.CancelRename();
    }
}