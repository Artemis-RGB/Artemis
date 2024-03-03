﻿using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.LayerEffects;

/// <summary>
///     For internal use only, please use <see cref="LayerEffect{T}" /> instead
/// </summary>
public abstract class BaseLayerEffect : BreakableModel, IDisposable, IStorageModel, IPluginFeatureDependent
{
    private ILayerEffectConfigurationDialog? _configurationDialog;
    private LayerEffectDescriptor _descriptor;
    private bool _hasBeenRenamed;
    private string _name;
    private int _order;
    private RenderProfileElement _profileElement;

    /// <inheritdoc />
    protected BaseLayerEffect()
    {
        // These are set right after construction to keep the constructor of inherited classes clean
        _profileElement = null!;
        _descriptor = null!;
        _name = null!;
        LayerEffectEntity = null!;
    }

    /// <summary>
    ///     Gets the
    /// </summary>
    public LayerEffectEntity LayerEffectEntity { get; internal set; }

    /// <summary>
    ///     Gets the profile element (such as layer or folder) this effect is applied to
    /// </summary>
    public RenderProfileElement ProfileElement
    {
        get => _profileElement;
        internal set => SetAndNotify(ref _profileElement, value);
    }

    /// <summary>
    ///     The name which appears in the editor
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetAndNotify(ref _name, value);
    }

    /// <summary>
    ///     Gets or sets whether the effect has been renamed by the user, if true consider refraining from changing the name
    ///     programatically
    /// </summary>
    public bool HasBeenRenamed
    {
        get => _hasBeenRenamed;
        set => SetAndNotify(ref _hasBeenRenamed, value);
    }

    /// <summary>
    ///     Gets the order in which this effect appears in the update loop and editor
    /// </summary>
    public int Order
    {
        get => _order;
        set => SetAndNotify(ref _order, value);
    }

    /// <summary>
    ///     Gets the <see cref="LayerEffectDescriptor" /> that registered this effect
    /// </summary>
    public LayerEffectDescriptor Descriptor
    {
        get => _descriptor;
        internal set => SetAndNotify(ref _descriptor, value);
    }

    /// <summary>
    ///     Gets or sets a configuration dialog complementing the regular properties
    /// </summary>
    public ILayerEffectConfigurationDialog? ConfigurationDialog
    {
        get => _configurationDialog;
        protected set => SetAndNotify(ref _configurationDialog, value);
    }

    /// <summary>
    ///     Gets the ID of the <see cref="LayerEffectProvider" /> that provided this effect
    /// </summary>
    public string ProviderId => Descriptor.Provider.Id;

    /// <summary>
    ///     Gets a reference to the layer property group without knowing it's type
    /// </summary>
    public virtual LayerEffectPropertyGroup? BaseProperties => null;

    /// <summary>
    ///     Gets a boolean indicating whether the layer effect is enabled or not
    /// </summary>
    public bool Enabled { get; private set; }

    /// <summary>
    ///     Gets a boolean indicating whether the layer effect is suspended or not
    /// </summary>
    public bool Suspended => BaseProperties is not {PropertiesInitialized: true} || !BaseProperties.IsEnabled;

    #region Overrides of BreakableModel

    /// <inheritdoc />
    public override string BrokenDisplayName => Name;

    #endregion

    /// <summary>
    ///     Called when the layer effect is activated
    /// </summary>
    public abstract void EnableLayerEffect();

    /// <summary>
    ///     Called when the layer effect is deactivated
    /// </summary>
    public abstract void DisableLayerEffect();

    /// <summary>
    ///     Called before rendering every frame, write your update logic here
    /// </summary>
    /// <param name="deltaTime"></param>
    public abstract void Update(double deltaTime);

    /// <summary>
    ///     Called before the layer or folder will be rendered
    /// </summary>
    /// <param name="canvas">The canvas used to render the frame</param>
    /// <param name="renderBounds">The bounds this layer/folder will render in</param>
    /// <param name="paint">The paint this layer/folder will use to render</param>
    public abstract void PreProcess(SKCanvas canvas, SKRect renderBounds, SKPaint paint);

    /// <summary>
    ///     Called after the layer of folder has been rendered
    /// </summary>
    /// <param name="canvas">The canvas used to render the frame</param>
    /// <param name="renderBounds">The bounds this layer/folder rendered in</param>
    /// <param name="paint">The paint this layer/folder used to render</param>
    public abstract void PostProcess(SKCanvas canvas, SKRect renderBounds, SKPaint paint);

    /// <summary>
    ///     Releases the unmanaged resources used by the object and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    ///     <see langword="true" /> to release both managed and unmanaged resources;
    ///     <see langword="false" /> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisableLayerEffect();
            BaseProperties?.Dispose();
        }
    }

    // Not only is this needed to initialize properties on the layer effects, it also prevents implementing anything
    // but LayerEffect<T> outside the core
    internal abstract void Initialize();

    internal void InternalUpdate(Timeline timeline)
    {
        BaseProperties?.Update(timeline);
        TryOrBreak(() => Update(timeline.Delta.TotalSeconds), "Failed to update");
    }

    internal void InternalPreProcess(SKCanvas canvas, SKRect renderBounds, SKPaint paint)
    {
        TryOrBreak(() => PreProcess(canvas, renderBounds, paint), "Failed to pre-process");
    }

    internal void InternalPostProcess(SKCanvas canvas, SKRect renderBounds, SKPaint paint)
    {
        TryOrBreak(() => PostProcess(canvas, renderBounds, paint), "Failed to post-process");
    }

    /// <summary>
    ///     Enables the layer effect if it isn't already enabled
    /// </summary>
    internal void InternalEnable()
    {
        if (Enabled)
            return;

        EnableLayerEffect();
        Enabled = true;
    }

    /// <summary>
    ///     Disables the layer effect if it isn't already disabled
    /// </summary>
    internal void InternalDisable()
    {
        if (!Enabled)
            return;

        DisableLayerEffect();
        Enabled = false;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public void Load()
    {
        Name = HasBeenRenamed ? LayerEffectEntity.Name : Descriptor.DisplayName;
        HasBeenRenamed = LayerEffectEntity.HasBeenRenamed;
        Order = LayerEffectEntity.Order;
    }

    /// <inheritdoc />
    public void Save()
    {
        LayerEffectEntity.Name = Name;
        LayerEffectEntity.HasBeenRenamed = HasBeenRenamed;
        LayerEffectEntity.Order = Order;

        if (Descriptor.IsPlaceholder)
            return;

        LayerEffectEntity.ProviderId = Descriptor.Provider.Id;
        LayerEffectEntity.EffectType = GetType().FullName ?? throw new InvalidOperationException();
        BaseProperties?.ApplyToEntity();
        LayerEffectEntity.PropertyGroup = BaseProperties?.PropertyGroupEntity;
    }

    #region Implementation of IPluginFeatureDependent

    /// <inheritdoc />
    public IEnumerable<PluginFeature> GetFeatureDependencies()
    {
        IEnumerable<PluginFeature> result = [Descriptor.Provider];
        if (BaseProperties != null)
            result = result.Concat(BaseProperties.GetFeatureDependencies());

        return result;
    }

    #endregion
}