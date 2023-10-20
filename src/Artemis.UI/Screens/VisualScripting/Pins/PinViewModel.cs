using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Avalonia;
using Avalonia.Media;
using DynamicData;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public abstract partial class PinViewModel : ActivatableViewModelBase
{
    private readonly INodeService _nodeService;
    [Notify] private Color _darkenedPinColor;
    [Notify] private Color _pinColor;
    [Notify] private Point _position;
    [Notify] private ReactiveCommand<IPin, Unit>? _removePin;

    protected PinViewModel(IPin pin, NodeScriptViewModel nodeScriptViewModel, INodeService nodeService, INodeEditorService nodeEditorService)
    {
        _nodeService = nodeService;

        Pin = pin;
        DisconnectPin = ReactiveCommand.Create(() => nodeEditorService.ExecuteCommand(nodeScriptViewModel.NodeScript, new DisconnectPins(Pin)));

        SourceList<IPin> connectedPins = new();
        this.WhenActivated(d =>
        {
            Observable.FromEventPattern<SingleValueEventArgs<IPin>>(x => Pin.PinConnected += x, x => Pin.PinConnected -= x)
                .Subscribe(e => connectedPins.Add(e.EventArgs.Value))
                .DisposeWith(d);
            Observable.FromEventPattern<SingleValueEventArgs<IPin>>(x => Pin.PinDisconnected += x, x => Pin.PinDisconnected -= x)
                .Subscribe(e => connectedPins.RemoveMany(connectedPins.Items.Where(p => p == e.EventArgs.Value)))
                .DisposeWith(d);
            Pin.WhenAnyValue(p => p.Type).Subscribe(_ => UpdatePinColor()).DisposeWith(d);
        });

        Connections = connectedPins.Connect().AsObservableList();
        connectedPins.AddRange(Pin.ConnectedTo);
    }

    public IObservableList<IPin> Connections { get; }
    public IPin Pin { get; }
    public ReactiveCommand<Unit, Unit> DisconnectPin { get; }

    public bool IsCompatibleWith(PinViewModel pinViewModel)
    {
        if (pinViewModel.Pin.Direction == Pin.Direction || pinViewModel.Pin.Node == Pin.Node)
            return false;

        return Pin.IsTypeCompatible(pinViewModel.Pin.Type);
    }

    private void UpdatePinColor()
    {
        TypeColorRegistration registration = _nodeService.GetTypeColorRegistration(Pin.Type);
        PinColor = registration.Color.ToColor();
        DarkenedPinColor = registration.DarkenedColor.ToColor();
    }
}