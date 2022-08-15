using System;
using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree;

public class LayerTreeItemView : ReactiveUserControl<LayerTreeItemViewModel>
{
    public LayerTreeItemView()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            ViewModel?.Rename.Subscribe(_ =>
            {
                this.Get<TextBox>("Input").Focus();
                this.Get<TextBox>("Input").SelectAll();
            }).DisposeWith(d);
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
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