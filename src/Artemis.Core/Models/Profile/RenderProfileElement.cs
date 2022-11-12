using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.LayerEffects;
using Artemis.Core.LayerEffects.Placeholder;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Conditions;
using SkiaSharp;

namespace Artemis.Core;

/// <summary>
///     Represents an element of a <see cref="Profile" /> that has advanced rendering capabilities
/// </summary>
public abstract class RenderProfileElement : ProfileElement
{
    private SKRectI _bounds;
    private SKPath? _path;

    internal RenderProfileElement(ProfileElement parent, Profile profile) : base(profile)
    {
        _layerEffects = new List<BaseLayerEffect>();
        _displayCondition = new AlwaysOnCondition(this);
        Timeline = new Timeline();
        LayerEffects = new ReadOnlyCollection<BaseLayerEffect>(_layerEffects);
        Parent = parent ?? throw new ArgumentNullException(nameof(parent));

        LayerEffectStore.LayerEffectAdded += LayerEffectStoreOnLayerEffectAdded;
        LayerEffectStore.LayerEffectRemoved += LayerEffectStoreOnLayerEffectRemoved;
    }

    /// <summary>
    ///     Gets a boolean indicating whether this render element and its layers/brushes are enabled
    /// </summary>
    public bool Enabled { get; protected set; }

    /// <summary>
    ///     Gets a boolean indicating whether this render element and its layers/brushes should be enabled
    /// </summary>
    public abstract bool ShouldBeEnabled { get; }

    /// <summary>
    ///     Creates a list of all layer properties present on this render element
    /// </summary>
    /// <returns>A list of all layer properties present on this render element</returns>
    public abstract List<ILayerProperty> GetAllLayerProperties();

    /// <summary>
    ///     Occurs when a layer effect has been added or removed to this render element
    /// </summary>
    public event EventHandler? LayerEffectsUpdated;

    /// <summary>
    ///     Overrides the main timeline to the specified time and clears any extra time lines
    /// </summary>
    /// <param name="position">The position to set the timeline to</param>
    public abstract void OverrideTimelineAndApply(TimeSpan position);

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        LayerEffectStore.LayerEffectAdded -= LayerEffectStoreOnLayerEffectAdded;
        LayerEffectStore.LayerEffectRemoved -= LayerEffectStoreOnLayerEffectRemoved;

        foreach (BaseLayerEffect baseLayerEffect in LayerEffects)
            baseLayerEffect.Dispose();

        if (DisplayCondition is IDisposable disposable)
            disposable.Dispose();

        base.Dispose(disposing);
    }

    internal void LoadRenderElement()
    {
        Timeline = RenderElementEntity.Timeline != null
            ? new Timeline(RenderElementEntity.Timeline)
            : new Timeline();

        DisplayCondition = RenderElementEntity.DisplayCondition switch
        {
            AlwaysOnConditionEntity entity => new AlwaysOnCondition(entity, this),
            PlayOnceConditionEntity entity => new PlayOnceCondition(entity, this),
            StaticConditionEntity entity => new StaticCondition(entity, this),
            EventConditionEntity entity => new EventCondition(entity, this),
            _ => DisplayCondition
        };

        LoadLayerEffects();
    }

    internal void SaveRenderElement()
    {
        RenderElementEntity.LayerEffects.Clear();
        foreach (BaseLayerEffect baseLayerEffect in LayerEffects)
        {
            baseLayerEffect.Save();
            RenderElementEntity.LayerEffects.Add(baseLayerEffect.LayerEffectEntity);
        }

        // Condition
        DisplayCondition?.Save();
        RenderElementEntity.DisplayCondition = DisplayCondition?.Entity;

        // Timeline
        RenderElementEntity.Timeline = Timeline?.Entity;
        Timeline?.Save();
    }

    internal void LoadNodeScript()
    {
        if (DisplayCondition is INodeScriptCondition scriptCondition)
            scriptCondition.LoadNodeScript();

        foreach (ILayerProperty layerProperty in GetAllLayerProperties())
            layerProperty.BaseDataBinding.LoadNodeScript();
    }

    private void OnLayerEffectsUpdated()
    {
        LayerEffectsUpdated?.Invoke(this, EventArgs.Empty);
    }

    #region Timeline

    /// <summary>
    ///     Gets the timeline associated with this render element
    /// </summary>
    public Timeline Timeline { get; private set; }

    /// <summary>
    ///     Updates the <see cref="Timeline" /> according to the provided <paramref name="deltaTime" /> and current display
    ///     condition
    /// </summary>
    protected void UpdateTimeline(double deltaTime)
    {
        DisplayCondition.UpdateTimeline(deltaTime);
    }

    #endregion

    #region Properties

    internal abstract RenderElementEntity RenderElementEntity { get; }

    /// <summary>
    ///     Gets the parent of this element
    /// </summary>
    public new ProfileElement Parent
    {
        get => base.Parent!;
        internal set
        {
            base.Parent = value;
            OnPropertyChanged(nameof(Parent));
        }
    }

    /// <summary>
    ///     Gets the path containing all the LEDs this entity is applied to, any rendering outside the entity Path is
    ///     clipped.
    /// </summary>
    public SKPath? Path
    {
        get => _path;
        protected set
        {
            SetAndNotify(ref _path, value);
            // I can't really be sure about the performance impact of calling Bounds often but
            // SkiaSharp calls SkiaApi.sk_path_get_bounds (Handle, &rect); which sounds expensive
            Bounds = SKRectI.Round(value?.Bounds ?? SKRect.Empty);
        }
    }

    /// <summary>
    ///     The bounds of this entity
    /// </summary>
    public SKRectI Bounds
    {
        get => _bounds;
        private set => SetAndNotify(ref _bounds, value);
    }

    #endregion

    #region State

    /// <summary>
    ///     Enables the render element and its brushes and effects
    /// </summary>
    public abstract void Disable();

    /// <summary>
    ///     Disables the render element and its brushes and effects
    /// </summary>
    public abstract void Enable();

    #endregion

    #region Effect management

    private readonly List<BaseLayerEffect> _layerEffects;

    /// <summary>
    ///     Gets a read-only collection of the layer effects on this entity
    /// </summary>
    public ReadOnlyCollection<BaseLayerEffect> LayerEffects { get; }

    /// <summary>
    ///     Adds a the provided layer effect to the render profile element
    /// </summary>
    /// <param name="layerEffect">The effect to add.</param>
    public void AddLayerEffect(BaseLayerEffect layerEffect)
    {
        if (layerEffect == null)
            throw new ArgumentNullException(nameof(layerEffect));

        // Ensure something needs to be done
        if (_layerEffects.Contains(layerEffect))
            return;

        // Make sure the layer effect is tied to this element
        layerEffect.ProfileElement = this;
        _layerEffects.Add(layerEffect);

        // Update the order on the effects
        OrderEffects();
        OnLayerEffectsUpdated();
    }

    /// <summary>
    ///     Removes the provided layer effect.
    /// </summary>
    /// <param name="layerEffect">The effect to remove.</param>
    public void RemoveLayerEffect(BaseLayerEffect layerEffect)
    {
        if (layerEffect == null)
            throw new ArgumentNullException(nameof(layerEffect));

        // Ensure something needs to be done
        if (!_layerEffects.Contains(layerEffect))
            return;

        // Remove the effect from the layer
        _layerEffects.Remove(layerEffect);

        // Update the order on the remaining effects
        OrderEffects();
        OnLayerEffectsUpdated();
    }

    private void LoadLayerEffects()
    {
        foreach (BaseLayerEffect baseLayerEffect in _layerEffects)
            baseLayerEffect.Dispose();
        _layerEffects.Clear();

        foreach (LayerEffectEntity layerEffectEntity in RenderElementEntity.LayerEffects.OrderBy(e => e.Order))
            LoadLayerEffect(layerEffectEntity);
    }

    private void LoadLayerEffect(LayerEffectEntity layerEffectEntity)
    {
        LayerEffectDescriptor? descriptor = LayerEffectStore.Get(layerEffectEntity.ProviderId, layerEffectEntity.EffectType)?.LayerEffectDescriptor;
        BaseLayerEffect layerEffect;
        // Create an instance with the descriptor
        if (descriptor != null)
        {
            layerEffect = descriptor.CreateInstance(this, layerEffectEntity);
        }
        // If no descriptor was found and there was no existing placeholder, create a placeholder
        else
        {
            descriptor = PlaceholderLayerEffectDescriptor.Create();
            layerEffect = descriptor.CreateInstance(this, layerEffectEntity);
        }

        _layerEffects.Add(layerEffect);
    }

    private void ReplaceLayerEffectWithPlaceholder(BaseLayerEffect layerEffect)
    {
        int index = _layerEffects.IndexOf(layerEffect);
        if (index == -1)
            return;

        LayerEffectDescriptor descriptor = PlaceholderLayerEffectDescriptor.Create();
        BaseLayerEffect placeholder = descriptor.CreateInstance(this, layerEffect.LayerEffectEntity);
        _layerEffects[index] = placeholder;
        layerEffect.Dispose();
        OnLayerEffectsUpdated();
    }

    private void ReplacePlaceholderWithLayerEffect(PlaceholderLayerEffect placeholder)
    {
        int index = _layerEffects.IndexOf(placeholder);
        if (index == -1)
            return;

        LayerEffectDescriptor? descriptor = LayerEffectStore.Get(placeholder.OriginalEntity.ProviderId, placeholder.OriginalEntity.EffectType)?.LayerEffectDescriptor;
        if (descriptor == null)
            throw new ArtemisCoreException("Can't replace a placeholder effect because the real effect isn't available.");

        BaseLayerEffect layerEffect = descriptor.CreateInstance(this, placeholder.OriginalEntity);
        _layerEffects[index] = layerEffect;
        placeholder.Dispose();
        OnLayerEffectsUpdated();
    }

    private void OrderEffects()
    {
        int index = 0;
        foreach (BaseLayerEffect baseLayerEffect in LayerEffects.OrderBy(e => e.Order))
        {
            baseLayerEffect.Order = index + 1;
            index++;
        }

        _layerEffects.Sort((a, b) => a.Order.CompareTo(b.Order));
    }

    private void LayerEffectStoreOnLayerEffectRemoved(object? sender, LayerEffectStoreEvent e)
    {
        // Find effects that just got disabled and replace them with placeholders
        List<BaseLayerEffect> affectedLayerEffects = _layerEffects.Where(e.Registration.Matches).ToList();

        if (!affectedLayerEffects.Any())
            return;

        foreach (BaseLayerEffect baseLayerEffect in affectedLayerEffects)
            ReplaceLayerEffectWithPlaceholder(baseLayerEffect);
        OnLayerEffectsUpdated();
    }

    private void LayerEffectStoreOnLayerEffectAdded(object? sender, LayerEffectStoreEvent e)
    {
        // Find placeholders that just got enabled and replace them with real effects
        List<PlaceholderLayerEffect> affectedPlaceholders = LayerEffects
            .Where(l => l is PlaceholderLayerEffect ph && e.Registration.Matches(ph))
            .Cast<PlaceholderLayerEffect>()
            .ToList();

        if (!affectedPlaceholders.Any())
            return;

        foreach (PlaceholderLayerEffect placeholderLayerEffect in affectedPlaceholders)
            ReplacePlaceholderWithLayerEffect(placeholderLayerEffect);
        OnLayerEffectsUpdated();
    }

    #endregion

    #region Conditions

    /// <summary>
    ///     Gets whether the display conditions applied to this layer where met or not during last update
    ///     <para>Always true if the layer has no display conditions</para>
    /// </summary>
    public bool DisplayConditionMet
    {
        get => _displayConditionMet;
        private set => SetAndNotify(ref _displayConditionMet, value);
    }

    private bool _displayConditionMet;

    /// <summary>
    ///     Gets or sets the display condition used to determine whether this element is active or not
    /// </summary>
    public ICondition DisplayCondition
    {
        get => _displayCondition;
        set => SetAndNotify(ref _displayCondition, value);
    }

    private ICondition _displayCondition;

    /// <summary>
    ///     Evaluates the display conditions on this element and applies any required changes to the <see cref="Timeline" />
    /// </summary>
    public void UpdateDisplayCondition()
    {
        if (Suspended)
        {
            DisplayConditionMet = false;
            return;
        }

        DisplayCondition.Update();
        DisplayConditionMet = DisplayCondition.IsMet;
    }

    #endregion
}