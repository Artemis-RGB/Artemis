using System;
using System.Collections.ObjectModel;
using System.Reactive;
using Artemis.Core;
using Artemis.Core.Services;
using Avalonia;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.SurfaceEditor.ViewModels
{
    public class SurfaceEditorViewModel : MainScreenViewModel
    {
        public SurfaceEditorViewModel(IScreen hostScreen, IRgbService rgbService) : base(hostScreen, "surface-editor")
        {
            DisplayName = "Surface Editor";
            Devices = new ObservableCollection<ArtemisDevice>(rgbService.Devices);

            UpdateSelection = ReactiveCommand.Create<Rect>(ExecuteUpdateSelection);
            ApplySelection = ReactiveCommand.Create<Rect>(ExecuteApplySelection);
        }

        public ObservableCollection<ArtemisDevice> Devices { get; }
        public ReactiveCommand<Rect, Unit> UpdateSelection { get; }
        public ReactiveCommand<Rect, Unit> ApplySelection { get; }

        private void ExecuteUpdateSelection(Rect rect)
        {

        }

        private void ExecuteApplySelection(Rect rect)
        {

        }
    }
}