using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Avalonia.Controls.Mixins;
using Avalonia.Input;
using DynamicData;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Timeline;

public class TimelineKeyframeViewModel<T> : ActivatableViewModelBase, ITimelineKeyframeViewModel
{
    private readonly IProfileEditorService _profileEditorService;

    private double _x;
    private string _timestamp;
    private ObservableAsPropertyHelper<bool>? _isSelected;

    public TimelineKeyframeViewModel(LayerPropertyKeyframe<T> layerPropertyKeyframe, IProfileEditorService profileEditorService)
    {
        _profileEditorService = profileEditorService;
        _timestamp = "0.000";
        LayerPropertyKeyframe = layerPropertyKeyframe;
        EasingViewModels = new ObservableCollection<TimelineEasingViewModel>();

        this.WhenActivated(d =>
        {
            _isSelected = profileEditorService.ConnectToKeyframes().ToCollection().Select(keyframes => keyframes.Contains(LayerPropertyKeyframe)).ToProperty(this, vm => vm.IsSelected).DisposeWith(d);
            profileEditorService.ConnectToKeyframes();
            profileEditorService.PixelsPerSecond.Subscribe(p =>
            {
                _pixelsPerSecond = p;
                profileEditorService.PixelsPerSecond.Subscribe(_ => Update()).DisposeWith(d);
            }).DisposeWith(d);

            this.WhenAnyValue(vm => vm.LayerPropertyKeyframe.Position).Subscribe(_ => Update()).DisposeWith(d);
        });
    }

    public LayerPropertyKeyframe<T> LayerPropertyKeyframe { get; }
    public ObservableCollection<TimelineEasingViewModel> EasingViewModels { get; }

    public double X
    {
        get => _x;
        set => this.RaiseAndSetIfChanged(ref _x, value);
    }

    public string Timestamp
    {
        get => _timestamp;
        set => this.RaiseAndSetIfChanged(ref _timestamp, value);
    }

    public bool IsSelected => _isSelected?.Value ?? false;
    public TimeSpan Position => LayerPropertyKeyframe.Position;
    public ILayerPropertyKeyframe Keyframe => LayerPropertyKeyframe;

    public void Update()
    {
        X = _pixelsPerSecond * LayerPropertyKeyframe.Position.TotalSeconds;
        Timestamp = $"{Math.Floor(LayerPropertyKeyframe.Position.TotalSeconds):00}.{LayerPropertyKeyframe.Position.Milliseconds:000}";
    }

    /// <inheritdoc />
    public void Select(bool expand, bool toggle)
    {
        _profileEditorService.SelectKeyframe(Keyframe, expand, toggle);
    }

    public TimeSpan GetTimeSpanAtPosition(double x)
    {
        return TimeSpan.FromSeconds(x / _pixelsPerSecond);
    }

    #region Context menu actions

    public void Duplicate()
    {
        throw new NotImplementedException();
    }

    public void Copy()
    {
        throw new NotImplementedException();
    }

    public void Paste()
    {
        throw new NotImplementedException();
    }

    public void Delete()
    {
        _profileEditorService.ExecuteCommand(new DeleteKeyframe<T>(LayerPropertyKeyframe));
    }

    #endregion

    #region Movement

    private TimeSpan? _offset;
    private TimeSpan? _startPosition;
    private double _pixelsPerSecond;

    public void StartMovement(ITimelineKeyframeViewModel source)
    {
        _startPosition = LayerPropertyKeyframe.Position;
        if (source != this)
            _offset = LayerPropertyKeyframe.Position - source.Position;
    }

    public void UpdateMovement(TimeSpan position)
    {
        if (_offset == null)
            UpdatePosition(position);
        else
            UpdatePosition(position + _offset.Value);
    }

    public void FinishMovement()
    {
        MoveKeyframe command = _startPosition != null
            ? new MoveKeyframe(Keyframe, Keyframe.Position, _startPosition.Value)
            : new MoveKeyframe(Keyframe, Keyframe.Position);

        _startPosition = null;
        _offset = null;

        _profileEditorService.ExecuteCommand(command);
    }

    private void UpdatePosition(TimeSpan position)
    {
        if (position < TimeSpan.Zero)
            LayerPropertyKeyframe.Position = TimeSpan.Zero;
        else if (position > LayerPropertyKeyframe.LayerProperty.ProfileElement.Timeline.Length)
            LayerPropertyKeyframe.Position = LayerPropertyKeyframe.LayerProperty.ProfileElement.Timeline.Length;
        else
            LayerPropertyKeyframe.Position = position;
    }

    #endregion

    #region Easing

    public void PopulateEasingViewModels()
    {
        if (EasingViewModels.Any())
            return;

        EasingViewModels.AddRange(Enum.GetValues(typeof(Easings.Functions))
            .Cast<Easings.Functions>()
            .Select(e => new TimelineEasingViewModel(e, Keyframe)));
    }
    
    public void SelectEasingFunction(Easings.Functions easingFunction)
    {
        _profileEditorService.ExecuteCommand(new ChangeKeyframeEasing(Keyframe, easingFunction));
    }

    #endregion
}

