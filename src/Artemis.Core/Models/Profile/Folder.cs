using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.LayerEffects;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.Abstract;
using SkiaSharp;

namespace Artemis.Core;

/// <summary>
///     Represents a folder in a <see cref="Profile" />
/// </summary>
public sealed class Folder : RenderProfileElement
{
    // Turns any alpha above zero into full alpha
    private static readonly SKImageFilter ContentCoverage = SKImageFilter.CreateColorFilter(SKColorFilter.CreateColorMatrix([
        0, 0, 0, 0, 0,
        0, 0, 0, 0, 0,
        0, 0, 0, 0, 0,
        0, 0, 0, 255, 0
    ]));

    private bool _isExpanded;
    private FolderGeneralProperties _general = new();

    /// <summary>
    ///     Creates a new instance of the <see cref="Folder" /> class with the provided <paramref name="parent" />.
    ///     <para>
    ///         The folder is not added to the child collection of the parent, use <see cref="ProfileElement.AddChild" /> to
    ///         add it.
    ///     </para>
    /// </summary>
    /// <param name="parent">The parent of the folder</param>
    /// <param name="name">The name of the folder</param>
    public Folder(ProfileElement parent, string name) : base(parent, parent.Profile)
    {
        FolderEntity = new FolderEntity();
        EntityId = Guid.NewGuid();

        Profile = Parent.Profile;
        Name = name;

        InitializeGeneral();
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="Folder" /> class based on the provided folder entity
    /// </summary>
    /// <param name="profile">The profile the folder belongs to</param>
    /// <param name="parent">The parent of the folder</param>
    /// <param name="folderEntity">The entity of the folder</param>
    public Folder(Profile profile, ProfileElement parent, FolderEntity folderEntity) : base(parent, parent.Profile)
    {
        FolderEntity = folderEntity;
        EntityId = folderEntity.Id;

        Profile = profile;
        Name = folderEntity.Name;
        IsExpanded = folderEntity.IsExpanded;
        Suspended = folderEntity.Suspended;
        Order = folderEntity.Order;

        Load();
        InitializeGeneral();
    }

    /// <summary>
    ///     Gets a boolean indicating whether this folder is at the root of the profile tree
    /// </summary>
    public bool IsRootFolder => Parent == Profile;

    /// <summary>
    ///     Gets or sets a boolean indicating whether this folder is expanded
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetAndNotify(ref _isExpanded, value);
    }

    /// <summary>
    ///     Gets the general properties of the folder.
    /// </summary>
    [PropertyGroupDescription(Identifier = "General", Name = "General", Description = "A collection of general properties")]
    public FolderGeneralProperties General
    {
        get => _general;
        private set => SetAndNotify(ref _general, value);
    }

    /// <summary>
    ///     Gets a value indicating whether this folder currently renders its children straight onto what is below it.
    /// </summary>
    /// <remarks>
    ///     Only when the blend mode is <see cref="FolderBlendMode.PassThrough" /> and every effect on the folder, including
    ///     disabled ones, draws only around its content.
    /// </remarks>
    public bool RendersPassThrough
    {
        get
        {
            if (BlendMode != FolderBlendMode.PassThrough)
                return false;
            foreach (BaseLayerEffect layerEffect in LayerEffects)
            {
                // Suspended effects count too, their enabled state can be animated and isolation must not flip with it
                if (!layerEffect.DrawsOnlyAroundContent)
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    ///     Gets how this folder blends into what is below it, which for the root folder is the blend mode of the profile.
    /// </summary>
    public FolderBlendMode BlendMode
    {
        get
        {
            if (IsRootFolder)
                return Profile.Configuration.BlendMode;
            return General.PropertiesInitialized ? General.BlendMode.CurrentValue : FolderBlendMode.SrcOver;
        }
    }

    /// <summary>
    ///     Gets the folder entity this folder uses for persistent storage
    /// </summary>
    public FolderEntity FolderEntity { get; internal set; }

    /// <inheritdoc />
    public override bool ShouldBeEnabled => !Suspended && DisplayConditionMet && !Timeline.IsFinished;

    internal override RenderElementEntity RenderElementEntity => FolderEntity;

    /// <inheritdoc />
    public override List<ILayerProperty> GetAllLayerProperties()
    {
        List<ILayerProperty> result = [..General.GetAllLayerProperties()];
        foreach (BaseLayerEffect layerEffect in LayerEffects)
        {
            if (layerEffect.BaseProperties != null)
                result.AddRange(layerEffect.BaseProperties.GetAllLayerProperties());
        }

        return result;
    }

    /// <inheritdoc />
    public override void Update(double deltaTime)
    {
        if (Disposed)
            throw new ObjectDisposedException("Folder");

        if (Timeline.IsOverridden)
        {
            Timeline.ClearOverride();
            return;
        }

        try
        {
            UpdateDisplayCondition();
            UpdateTimeline(deltaTime);

            if (ShouldBeEnabled)
                Enable();
            else if (Suspended || Timeline.IsFinished)
                Disable();

            General.Update(Timeline);
            foreach (BaseLayerEffect baseLayerEffect in LayerEffects)
            {
                if (!baseLayerEffect.Suspended)
                    baseLayerEffect.InternalUpdate(Timeline);
            }

            foreach (ProfileElement child in Children)
                child.Update(deltaTime);
        }
        finally
        {
            Timeline.ClearDelta();
        }
    }

    /// <inheritdoc />
    public override void Reset()
    {
        UpdateDisplayCondition();

        if (DisplayConditionMet)
            Timeline.JumpToStart();
        else
            Timeline.JumpToEnd();

        foreach (ProfileElement child in Children)
            child.Reset();
    }

    /// <inheritdoc />
    public override void AddChild(ProfileElement child, int? order = null)
    {
        if (Disposed)
            throw new ObjectDisposedException("Folder");

        base.AddChild(child, order);
        CalculateRenderProperties();
    }

    /// <inheritdoc />
    public override void RemoveChild(ProfileElement child)
    {
        if (Disposed)
            throw new ObjectDisposedException("Folder");

        base.RemoveChild(child);
        CalculateRenderProperties();
    }

    /// <summary>
    ///     Creates a deep copy of the folder
    /// </summary>
    /// <returns>The newly created copy</returns>
    public Folder CreateCopy()
    {
        if (Parent == null)
            throw new ArtemisCoreException("Cannot create a copy of a folder without a parent");

        FolderEntity entityCopy = CoreJson.Deserialize<FolderEntity>(CoreJson.Serialize(FolderEntity))!;
        entityCopy.Id = Guid.NewGuid();
        entityCopy.Name += " - Copy";

        // TODO Children

        return new Folder(Profile, Parent, entityCopy);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"[Folder] {nameof(Name)}: {Name}, {nameof(Order)}: {Order}";
    }

    /// <inheritdoc />
    public override IEnumerable<PluginFeature> GetFeatureDependencies()
    {
        return General.GetFeatureDependencies()
            .Concat(LayerEffects.SelectMany(e => e.GetFeatureDependencies()))
            .Concat(Children.SelectMany(c => c.GetFeatureDependencies()))
            .Concat(DisplayCondition.GetFeatureDependencies());
    }

    #region Rendering

    /// <inheritdoc />
    public override void Render(SKCanvas canvas, SKPointI basePosition, ProfileElement? editorFocus)
    {
        if (Disposed)
            throw new ObjectDisposedException("Folder");

        // Ensure the folder is ready
        if (!Enabled || Path == null)
            return;

        // No point rendering if all children are disabled
        if (!Children.Any(c => c is RenderProfileElement {Enabled: true}))
            return;

        // If the editor focus is on this folder, discard further focus for children to effectively focus the entire folder and all descendants
        if (editorFocus == this)
            editorFocus = null;

        if (!RendersPassThrough)
        {
            RenderInLayer(canvas, basePosition, editorFocus, false);
            return;
        }

        // Effects only draw around the children here, so they get a pass of their own that leaves the children out
        foreach (BaseLayerEffect baseLayerEffect in LayerEffects)
        {
            if (baseLayerEffect.Suspended)
                continue;
            RenderInLayer(canvas, basePosition, editorFocus, true);
            break;
        }

        canvas.Save();
        try
        {
            RenderChildren(canvas, basePosition, editorFocus);
        }
        finally
        {
            canvas.Restore();
        }
    }

    private void RenderInLayer(SKCanvas canvas, SKPointI basePosition, ProfileElement? editorFocus, bool onlyAroundContent)
    {
        SKPaint layerPaint = new() {FilterQuality = SKFilterQuality.Low};
        if (!onlyAroundContent)
            layerPaint.BlendMode = FolderGeneralProperties.ToSKBlendMode(BlendMode);
        SKImageFilter? effectFilter = null;
        try
        {
            SKRectI rendererBounds = SKRectI.Create(0, 0, Bounds.Width, Bounds.Height);
            foreach (BaseLayerEffect baseLayerEffect in LayerEffects)
            {
                if (!baseLayerEffect.Suspended)
                    baseLayerEffect.InternalPreProcess(canvas, rendererBounds, layerPaint);
            }

            // No point rendering if the alpha was set to zero by one of the effects
            if (layerPaint.Color.Alpha == 0)
                return;

            // Cut everything the children cover out of what the effects drew, translucent parts included
            if (onlyAroundContent)
            {
                effectFilter = layerPaint.ImageFilter;
                layerPaint.ImageFilter = SKImageFilter.CreateBlendMode(SKBlendMode.DstOut, effectFilter, ContentCoverage);
            }

            canvas.SaveLayer(layerPaint);
            try
            {
                RenderChildren(canvas, basePosition, editorFocus);

                foreach (BaseLayerEffect baseLayerEffect in LayerEffects)
                {
                    if (!baseLayerEffect.Suspended)
                        baseLayerEffect.InternalPostProcess(canvas, rendererBounds, layerPaint);
                }
            }
            finally
            {
                canvas.Restore();
            }
        }
        finally
        {
            effectFilter?.Dispose();
            layerPaint.DisposeSelfAndProperties();
        }
    }

    private void RenderChildren(SKCanvas canvas, SKPointI basePosition, ProfileElement? editorFocus)
    {
        canvas.Translate(Bounds.Left - basePosition.X, Bounds.Top - basePosition.Y);

        // Iterate the children in reverse because the first layer must be rendered last to end up on top
        for (int index = Children.Count - 1; index > -1; index--)
            Children[index].Render(canvas, new SKPointI(Bounds.Left, Bounds.Top), editorFocus);
    }

    #endregion

    /// <inheritdoc />
    public override void Enable()
    {
        if (Enabled)
            return;
        
        // Not enabling children, they'll enable themselves during their own Update
        foreach (BaseLayerEffect baseLayerEffect in LayerEffects)
            baseLayerEffect.InternalEnable();

        Enabled = true;
    }

    /// <inheritdoc />
    public override void Disable()
    {
        if (!Enabled)
            return;
        
        foreach (BaseLayerEffect baseLayerEffect in LayerEffects)
            baseLayerEffect.InternalDisable();

        // Disabling children since their Update won't get called with their parent disabled
        foreach (ProfileElement profileElement in Children)
        {
            if (profileElement is RenderProfileElement renderProfileElement)
                renderProfileElement.Disable();
        }

        Enabled = false;
    }

    /// <inheritdoc />
    public override void OverrideTimelineAndApply(TimeSpan position)
    {
        DisplayCondition.OverrideTimeline(position);
        General.Update(Timeline);
        foreach (BaseLayerEffect baseLayerEffect in LayerEffects.Where(e => !e.Suspended))
            baseLayerEffect.InternalUpdate(Timeline);
    }

    /// <summary>
    ///     Occurs when a property affecting the rendering properties of this folder has been updated
    /// </summary>
    public event EventHandler? RenderPropertiesUpdated;

    #region Overrides of BreakableModel

    /// <inheritdoc />
    public override IEnumerable<IBreakableModel> GetBrokenHierarchy()
    {
        return LayerEffects.Where(e => e.BrokenState != null);
    }

    #endregion

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        Disposed = true;

        Disable();
        foreach (ProfileElement profileElement in Children)
            profileElement.Dispose();
        _general.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeGeneral()
    {
        PropertyGroupDescriptionAttribute generalAttribute = (PropertyGroupDescriptionAttribute) Attribute.GetCustomAttribute(
            GetType().GetProperty(nameof(General))!,
            typeof(PropertyGroupDescriptionAttribute)
        )!;

        FolderEntity.GeneralPropertyGroup ??= new PropertyGroupEntity {Identifier = generalAttribute.Identifier!};
        General.Initialize(this, null, generalAttribute, FolderEntity.GeneralPropertyGroup);
    }

    internal void CalculateRenderProperties()
    {
        if (Disposed)
            throw new ObjectDisposedException("Folder");

        SKPath path = new() {FillType = SKPathFillType.Winding};
        foreach (ProfileElement child in Children)
        {
            if (child is RenderProfileElement effectChild && effectChild.Path != null)
                path.AddPath(effectChild.Path);
        }

        Path = path;

        // Folder render properties are based on child paths and thus require an update
        if (Parent is Folder folder)
            folder.CalculateRenderProperties();

        OnRenderPropertiesUpdated();
    }

    internal override void Load()
    {
        Reset();

        // Load child folders
        foreach (FolderEntity childFolder in Profile.ProfileEntity.Folders.Where(f => f.ParentId == EntityId))
            ChildrenList.Add(new Folder(Profile, this, childFolder));
        // Load child layers
        foreach (LayerEntity childLayer in Profile.ProfileEntity.Layers.Where(f => f.ParentId == EntityId))
            ChildrenList.Add(new Layer(Profile, this, childLayer));

        // Ensure order integrity, should be unnecessary but no one is perfect specially me
        ChildrenList.Sort((a, b) => a.Order.CompareTo(b.Order));
        for (int index = 0; index < ChildrenList.Count; index++)
            ChildrenList[index].Order = index + 1;

        LoadRenderElement();
    }

    internal override void Save()
    {
        if (Disposed)
            throw new ObjectDisposedException("Folder");

        FolderEntity.Id = EntityId;
        FolderEntity.ParentId = Parent?.EntityId ?? new Guid();

        FolderEntity.Order = Order;
        FolderEntity.Name = Name;
        FolderEntity.IsExpanded = IsExpanded;
        FolderEntity.Suspended = Suspended;
        General.ApplyToEntity();

        SaveRenderElement();
    }

    private void OnRenderPropertiesUpdated()
    {
        RenderPropertiesUpdated?.Invoke(this, EventArgs.Empty);
    }
}