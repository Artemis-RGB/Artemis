using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia.Controls.Mixins;
using Avalonia.Input;
using DynamicData;
using Humanizer;
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
            profileEditorService.PixelsPerSecond.Subscribe(p =>
            {
                _pixelsPerSecond = p;
                profileEditorService.PixelsPerSecond.Subscribe(_ => Update()).DisposeWith(d);
            }).DisposeWith(d);

            _isSelected = profileEditorService.ConnectToKeyframes().ToCollection().Select(keyframes => keyframes.Contains(LayerPropertyKeyframe)).ToProperty(this, vm => vm.IsSelected).DisposeWith(d);
            profileEditorService.ConnectToKeyframes();
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

    #region Context menu actions

    public void Delete(bool save = true)
    {
        throw new NotImplementedException();
    }

    #endregion


    #region Movement

    private TimeSpan? _offset;
    private double _pixelsPerSecond;

    public void ReleaseMovement()
    {
        _offset = null;
    }

    public void SaveOffsetToKeyframe(ITimelineKeyframeViewModel source)
    {
        if (source == this)
        {
            _offset = null;
            return;
        }

        if (_offset != null)
            return;

        _offset = LayerPropertyKeyframe.Position - source.Position;
    }

    public void ApplyOffsetToKeyframe(ITimelineKeyframeViewModel source)
    {
        if (source == this || _offset == null)
            return;

        UpdatePosition(source.Position + _offset.Value);
    }

    public void UpdatePosition(TimeSpan position)
    {
        throw new NotImplementedException();

        // if (position < TimeSpan.Zero)
        //     LayerPropertyKeyframe.Position = TimeSpan.Zero;
        // else if (position > _profileEditorService.SelectedProfileElement.Timeline.Length)
        //     LayerPropertyKeyframe.Position = _profileEditorService.SelectedProfileElement.Timeline.Length;
        // else
        //     LayerPropertyKeyframe.Position = position;

        Update();
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

public class ChangeKeyframeEasing : IProfileEditorCommand
{
    private readonly ILayerPropertyKeyframe _keyframe;
    private readonly Easings.Functions _easingFunction;
    private readonly Easings.Functions _originalEasingFunction;

    public ChangeKeyframeEasing(ILayerPropertyKeyframe keyframe, Easings.Functions easingFunction)
    {
        _keyframe = keyframe;
        _easingFunction = easingFunction;
        _originalEasingFunction = keyframe.EasingFunction;
    }

    #region Implementation of IProfileEditorCommand

    /// <inheritdoc />
    public string DisplayName => "Change easing to " + _easingFunction.Humanize(LetterCasing.LowerCase);

    /// <inheritdoc />
    public void Execute()
    {
        _keyframe.EasingFunction = _easingFunction;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _keyframe.EasingFunction = _originalEasingFunction;
    }

    #endregion
}