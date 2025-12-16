using System;
using Artemis.UI.Shared.Controls;
using ReactiveUI.Avalonia;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public partial class SKPointPropertyInputView : ReactiveUserControl<SKPointPropertyInputViewModel>
{
    public SKPointPropertyInputView()
    {
        InitializeComponent();
    }


    private void DraggableNumberBox_OnDragStarted(DraggableNumberBox sender, EventArgs args)
    {
        ViewModel?.StartPreview();
    }

    private void DraggableNumberBox_OnDragFinished(DraggableNumberBox sender, EventArgs args)
    {
        ViewModel?.ApplyPreview();
    }
}