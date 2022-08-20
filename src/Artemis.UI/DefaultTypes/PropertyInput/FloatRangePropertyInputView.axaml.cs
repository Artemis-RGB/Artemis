using System;
using Artemis.UI.Shared.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.DefaultTypes.PropertyInput
{
    public partial class FloatRangePropertyInputView : ReactiveUserControl<FloatRangePropertyInputViewModel>
    {
        public FloatRangePropertyInputView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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
}
