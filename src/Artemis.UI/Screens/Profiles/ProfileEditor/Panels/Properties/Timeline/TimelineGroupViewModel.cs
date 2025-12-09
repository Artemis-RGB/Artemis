using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using DynamicData;
using DynamicData.Binding;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Profiles.ProfileEditor.Properties.Timeline;

public partial class TimelineGroupViewModel : ActivatableViewModelBase
{
    private int _pixelsPerSecond;
    [Notify] private ReadOnlyObservableCollection<double> _keyframePositions;

    public TimelineGroupViewModel(PropertyGroupViewModel propertyGroupViewModel, IProfileEditorService profileEditorService)
    {
        PropertyGroupViewModel = propertyGroupViewModel;
        _keyframePositions = new ReadOnlyObservableCollection<double>(new ObservableCollection<double>());

        ViewForMixins.WhenActivated((IActivatableViewModel) this, (CompositeDisposable d) =>
        {
            profileEditorService.PixelsPerSecond.Subscribe(p => _pixelsPerSecond = p).DisposeWith(d);

            PropertyGroupViewModel.WhenAnyValue<PropertyGroupViewModel, bool>(vm => vm.IsExpanded).Subscribe(_ => this.RaisePropertyChanged(nameof(Children))).DisposeWith(d);
            PropertyGroupViewModel.Keyframes
                .ToObservableChangeSet()
                .AutoRefreshOnObservable(_ => profileEditorService.PixelsPerSecond)
                .AutoRefreshOnObservable(k => k.WhenAnyValue(kv => kv.Position))
                .Transform(k => k.Position.TotalSeconds * _pixelsPerSecond, true)
                .Bind(out ReadOnlyObservableCollection<double> keyframePositions)
                .Subscribe()
                .DisposeWith(d);
            KeyframePositions = keyframePositions;
        });
    }

    public PropertyGroupViewModel PropertyGroupViewModel { get; }

    public ObservableCollection<PropertyViewModelBase>? Children => PropertyGroupViewModel.IsExpanded ? PropertyGroupViewModel.Children : null;
}