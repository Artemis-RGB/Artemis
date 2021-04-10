using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Artemis.Core.LayerBrushes
{
    /// <summary>
    ///     For internal use only, please use <see cref="LayerBrush{T}" /> or <see cref="RgbNetLayerBrush{T}" /> or instead
    /// </summary>
    public abstract class BaseLayerBrush : CorePropertyChanged, IDisposable
    {
        private LayerBrushType _brushType;
        private ILayerBrushConfigurationDialog? _configurationDialog;
        private LayerBrushDescriptor _descriptor;
        private Layer _layer;
        private bool _supportsTransformation = true;

        /// <summary>
        ///     Creates a new instance of the <see cref="BaseLayerBrush" /> class
        /// </summary>
        protected BaseLayerBrush()
        {
            // Both are set right after construction to keep the constructor of inherited classes clean
            _layer = null!;
            _descriptor = null!;
        }

        /// <summary>
        ///     Gets the layer this brush is applied to
        /// </summary>
        public Layer Layer
        {
            get => _layer;
            internal set => SetAndNotify(ref _layer, value);
        }

        /// <summary>
        ///     Gets the descriptor of this brush
        /// </summary>
        public LayerBrushDescriptor Descriptor
        {
            get => _descriptor;
            internal set => SetAndNotify(ref _descriptor, value);
        }

        /// <summary>
        ///     Gets or sets a configuration dialog complementing the regular properties
        /// </summary>
        public ILayerBrushConfigurationDialog? ConfigurationDialog
        {
            get => _configurationDialog;
            protected set => SetAndNotify(ref _configurationDialog, value);
        }

        /// <summary>
        ///     Gets the type of layer brush
        /// </summary>
        public LayerBrushType BrushType
        {
            get => _brushType;
            internal set => SetAndNotify(ref _brushType, value);
        }

        /// <summary>
        ///     Gets the ID of the <see cref="LayerBrushProvider" /> that provided this effect
        /// </summary>
        public string? ProviderId => Descriptor?.Provider.Id;

        /// <summary>
        ///     Gets a reference to the layer property group without knowing it's type
        /// </summary>
        public virtual LayerPropertyGroup? BaseProperties => null;

        /// <summary>
        ///     Gets a list of presets available to this layer brush
        /// </summary>
        public virtual List<ILayerBrushPreset>? Presets => null;

        /// <summary>
        ///     Gets the default preset used for new instances of this layer brush
        /// </summary>
        public virtual ILayerBrushPreset? DefaultPreset => Presets?.FirstOrDefault();

        /// <summary>
        ///     Gets a boolean indicating whether the layer brush is enabled or not
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        ///     Gets or sets whether the brush supports transformations
        ///     <para>Note: RGB.NET brushes can never be transformed and setting this to true will throw an exception</para>
        /// </summary>
        public bool SupportsTransformation
        {
            get => _supportsTransformation;
            protected set
            {
                if (value && BrushType == LayerBrushType.RgbNet)
                    throw new ArtemisPluginFeatureException(Descriptor?.Provider!, "An RGB.NET brush cannot support transformation");
                _supportsTransformation = value;
            }
        }

        /// <summary>
        ///     Called when the layer brush is activated
        /// </summary>
        public abstract void EnableLayerBrush();

        /// <summary>
        ///     Called when the layer brush is deactivated
        /// </summary>
        public abstract void DisableLayerBrush();

        /// <summary>
        ///     Called before rendering every frame, write your update logic here
        /// </summary>
        /// <param name="deltaTime">Seconds passed since last update</param>
        public abstract void Update(double deltaTime);

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
                DisableLayerBrush();
                BaseProperties?.Dispose();
            }
        }

        /// <summary>
        ///     Enables the layer brush if it isn't already enabled
        /// </summary>
        internal void InternalEnable()
        {
            if (Enabled)
                return;

            EnableLayerBrush();
            Enabled = true;
        }

        /// <summary>
        ///     Disables the layer brush if it isn't already disabled
        /// </summary>
        internal void InternalDisable()
        {
            if (!Enabled)
                return;

            DisableLayerBrush();
            Enabled = false;
        }

        // Not only is this needed to initialize properties on the layer brushes, it also prevents implementing anything
        // but LayerBrush<T> and RgbNetLayerBrush<T> outside the core
        internal abstract void Initialize();

        internal abstract void InternalRender(SKCanvas canvas, SKRect path, SKPaint paint);

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    ///     Describes the type of a layer brush
    /// </summary>
    public enum LayerBrushType
    {
        /// <summary>
        ///     A regular brush that users Artemis' SkiaSharp-based rendering engine
        /// </summary>
        Regular,

        /// <summary>
        ///     An RGB.NET brush that uses RGB.NET's per-LED rendering engine.
        /// </summary>
        RgbNet
    }
}