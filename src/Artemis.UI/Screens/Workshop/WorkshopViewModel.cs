﻿using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.VisualScripting;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.VisualScripting.Nodes;
using Avalonia.Input;
using Avalonia.Threading;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.Screens.Workshop
{
    public class WorkshopViewModel : MainScreenViewModel
    {
        private static NodeScript<bool>? _testScript = null;

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

            if (_testScript == null)
            {
                _testScript = new NodeScript<bool>("Test script", "A test script");
                INode exitNode = _testScript.Nodes.Last();
                exitNode.X = 300;
                exitNode.Y = 150;

                OrNode orNode = new() {X = 100, Y = 100};
                _testScript.AddNode(orNode);
                orNode.Result.ConnectTo(exitNode.Pins.First());
            }

            VisualEditorViewModel = nodeVmFactory.NodeScriptViewModel(_testScript);

            this.WhenActivated(d =>
            {
                DispatcherTimer updateTimer = new(TimeSpan.FromMilliseconds(20), DispatcherPriority.Normal, (_, _) => _testScript?.Run());
                updateTimer.Start();
                Disposable.Create(() => updateTimer.Stop()).DisposeWith(d);
            });
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