using System;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Extensions;
using Avalonia;
using Avalonia.Controls.Mixins;
using Avalonia.Media;
using DynamicData;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public abstract class PinViewModel : ActivatableViewModelBase
{
    private Point _position;

    protected PinViewModel(IPin pin, INodeService nodeService)
    {
        Pin = pin;

        TypeColorRegistration registration = nodeService.GetTypeColorRegistration(Pin.Type);
        PinColor = registration.Color.ToColor();
        DarkenedPinColor = registration.DarkenedColor.ToColor();

        SourceList<IPin> connectedPins = new();
        this.WhenActivated(d =>
        {
            Observable.FromEventPattern<SingleValueEventArgs<IPin>>(x => Pin.PinConnected += x, x => Pin.PinConnected -= x)
                .Subscribe(e => connectedPins.Add(e.EventArgs.Value))
                .DisposeWith(d);
            Observable.FromEventPattern<SingleValueEventArgs<IPin>>(x => Pin.PinDisconnected += x, x => Pin.PinDisconnected -= x)
                .Subscribe(e => connectedPins.Remove(e.EventArgs.Value))
                .DisposeWith(d);
        });

        Connections = connectedPins.Connect().AsObservableList();
        connectedPins.AddRange(Pin.ConnectedTo);
    }

    public IObservableList<IPin> Connections { get; }

    public IPin Pin { get; }
    public Color PinColor { get; }
    public Color DarkenedPinColor { get; }

    public Point Position
    {
        get => _position;
        set => RaiseAndSetIfChanged(ref _position, value);
    }

    public bool IsCompatibleWith(PinViewModel pinViewModel)
    {
        if (pinViewModel.Pin.Direction == Pin.Direction)
            return false;
        if (pinViewModel.Pin.Node == Pin.Node)
            return false;

        return Pin.Type == pinViewModel.Pin.Type
               || Pin.Type == typeof(Enum) && pinViewModel.Pin.Type.IsEnum
               || Pin.Direction == PinDirection.Input && Pin.Type == typeof(object)
               || Pin.Direction == PinDirection.Output && pinViewModel.Pin.Type == typeof(object);
    }
}