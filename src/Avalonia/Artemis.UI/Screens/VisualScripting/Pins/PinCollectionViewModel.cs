using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Avalonia.Controls.Mixins;
using DynamicData;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public abstract class PinCollectionViewModel : ActivatableViewModelBase
{
    private readonly INodePinVmFactory _nodePinVmFactory;

    protected PinCollectionViewModel(IPinCollection pinCollection, NodeScriptViewModel nodeScriptViewModel, INodePinVmFactory nodePinVmFactory, INodeEditorService nodeEditorService)
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

        AddPin = ReactiveCommand.Create(() => nodeEditorService.ExecuteCommand(nodeScriptViewModel.NodeScript, new AddPin(pinCollection)));
        RemovePin = ReactiveCommand.Create((IPin pin) => nodeEditorService.ExecuteCommand(nodeScriptViewModel.NodeScript, new RemovePin(pinCollection, pin)));
    }

    public IPinCollection PinCollection { get; }
    public ReactiveCommand<Unit, Unit> AddPin { get; }
    public ReactiveCommand<IPin, Unit> RemovePin { get; }

    public ObservableCollection<PinViewModel> PinViewModels { get; }

    private PinViewModel CreatePinViewModel(IPin pin)
    {
        PinViewModel vm = PinCollection.Direction == PinDirection.Input ? _nodePinVmFactory.InputPinViewModel(pin) : _nodePinVmFactory.OutputPinViewModel(pin);
        vm.RemovePin = RemovePin;
        return vm;
    }
}