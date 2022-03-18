using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Avalonia.Controls.Mixins;
using DynamicData;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public abstract class PinCollectionViewModel : ActivatableViewModelBase
{
    private readonly INodePinVmFactory _nodePinVmFactory;
    public IPinCollection PinCollection { get; }
    public ObservableCollection<PinViewModel> PinViewModels { get; }

    protected PinCollectionViewModel(IPinCollection pinCollection, INodePinVmFactory nodePinVmFactory)
    {
        _nodePinVmFactory = nodePinVmFactory;

        PinCollection = pinCollection;
        PinViewModels = new ObservableCollection<PinViewModel>();

        this.WhenActivated(d =>
        {
            PinViewModels.Clear();
            PinViewModels.AddRange(PinCollection.Select(CreatePinViewModel));

            Observable.FromEventPattern<SingleValueEventArgs<IPin>>(x => PinCollection.PinAdded += x, x => PinCollection.PinAdded -= x)
                .Subscribe(e => PinViewModels.Add(CreatePinViewModel(e.EventArgs.Value)))
                .DisposeWith(d);
            Observable.FromEventPattern<SingleValueEventArgs<IPin>>(x => PinCollection.PinRemoved += x, x => PinCollection.PinRemoved -= x)
                .Subscribe(e => PinViewModels.RemoveMany(PinViewModels.Where(p => p.Pin == e.EventArgs.Value).ToList()))
                .DisposeWith(d);
        });

        AddPin = ReactiveCommand.Create(() => PinCollection.AddPin());
    }

    public ReactiveCommand<Unit, IPin> AddPin { get; }

    private PinViewModel CreatePinViewModel(IPin pin)
    {
        return PinCollection.Direction == PinDirection.Input ? _nodePinVmFactory.InputPinViewModel(pin) : _nodePinVmFactory.OutputPinViewModel(pin);
    }
}