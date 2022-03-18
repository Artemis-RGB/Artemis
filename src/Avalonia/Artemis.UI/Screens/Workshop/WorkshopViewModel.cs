using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.VisualScripting;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.VisualScripting.Nodes;
using Avalonia.Input;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.Workshop
{
    public class WorkshopViewModel : MainScreenViewModel
    {
        private readonly INotificationService _notificationService;
        private StandardCursorType _selectedCursor;
        private readonly ObservableAsPropertyHelper<Cursor> _cursor;

        private ColorGradient _colorGradient = new()
        {
            new ColorGradientStop(new SKColor(0xFFFF6D00), 0f),
            new ColorGradientStop(new SKColor(0xFFFE6806), 0.2f),
            new ColorGradientStop(new SKColor(0xFFEF1788), 0.4f),
            new ColorGradientStop(new SKColor(0xFFEF1788), 0.6f),
            new ColorGradientStop(new SKColor(0xFF00FCCC), 0.8f),
            new ColorGradientStop(new SKColor(0xFF00FCCC), 1f),
        };

        public WorkshopViewModel(IScreen hostScreen, INotificationService notificationService, INodeVmFactory nodeVmFactory) : base(hostScreen, "workshop")
        {
            _notificationService = notificationService;
            _cursor = this.WhenAnyValue(vm => vm.SelectedCursor).Select(c => new Cursor(c)).ToProperty(this, vm => vm.Cursor);

            DisplayName = "Workshop";
            ShowNotification = ReactiveCommand.Create<NotificationSeverity>(ExecuteShowNotification);

            NodeScript<bool> testScript = new("Test script", "A test script");
            INode exitNode = testScript.Nodes.Last();
            exitNode.X = 200;
            exitNode.Y = 100;

            OrNode orNode = new() {X = 100, Y = 100};
            testScript.AddNode(orNode);
            orNode.Result.ConnectTo(exitNode.Pins.First());

            VisualEditorViewModel = nodeVmFactory.NodeScriptViewModel(testScript);
        }

        public NodeScriptViewModel VisualEditorViewModel { get; }

        public ReactiveCommand<NotificationSeverity, Unit> ShowNotification { get; set; }

        public StandardCursorType SelectedCursor
        {
            get => _selectedCursor;
            set => RaiseAndSetIfChanged(ref _selectedCursor, value);
        }

        public Cursor Cursor => _cursor.Value;

        public ColorGradient ColorGradient
        {
            get => _colorGradient;
            set => RaiseAndSetIfChanged(ref _colorGradient, value);
        }

        public void CreateRandomGradient()
        {
            ColorGradient = ColorGradient.GetRandom(6);
        }

        private void ExecuteShowNotification(NotificationSeverity severity)
        {
            _notificationService.CreateNotification().WithTitle("Test title").WithMessage("Test message").WithSeverity(severity).Show();
        }
    }
}