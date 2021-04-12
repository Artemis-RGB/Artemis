using System;
using Artemis.Core.Services;
using SkiaSharp;

namespace Artemis.Core.LayerEffects
{
    /// <summary>
    ///     For internal use only, please use <see cref="LayerEffect{T}" /> instead
    /// </summary>
    public abstract class BaseLayerEffect : CorePropertyChanged, IDisposable
    {
        private ILayerEffectConfigurationDialog? _configurationDialog;
        private LayerEffectDescriptor _descriptor;
        private bool _suspended;
        private Guid _entityId;
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
        }

        /// <summary>
        ///     Gets the unique ID of this effect
        /// </summary>
        public Guid EntityId
        {
            get => _entityId;
            internal set => SetAndNotify(ref _entityId, value);
        }

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
        ///     Gets or sets the suspended state, if suspended the effect is skipped in render and update
        /// </summary>
        public bool Suspended
        {
            get => _suspended;
            set => SetAndNotify(ref _suspended, value);
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
        ///     Gets the ID of the <see cref="LayerEffectProvider"/> that provided this effect
        /// </summary>
        public string ProviderId => Descriptor.Provider.Id;

        /// <summary>
        ///     Gets a reference to the layer property group without knowing it's type
        /// </summary>
        public virtual LayerPropertyGroup? BaseProperties => null;

        internal string PropertyRootPath => $"LayerEffect.{EntityId}.{GetType().Name}.";

        /// <summary>
        ///     Gets a boolean indicating whether the layer effect is enabled or not
        /// </summary>
        public bool Enabled { get; private set; }

        #region IDisposable

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

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

        // Not only is this needed to initialize properties on the layer effects, it also prevents implementing anything
        // but LayerEffect<T> outside the core
        internal abstract void Initialize();

        internal virtual string GetEffectTypeName() => GetType().Name;

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
    }
}