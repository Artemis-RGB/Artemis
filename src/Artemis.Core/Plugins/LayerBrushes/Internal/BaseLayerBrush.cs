﻿using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Services.Interfaces;
using SkiaSharp;
using Stylet;

namespace Artemis.Core.Plugins.LayerBrushes.Internal
{
    /// <summary>
    ///     For internal use only, please use <see cref="LayerBrush{T}" /> or <see cref="RgbNetLayerBrush{T}" /> or instead
    /// </summary>
    public abstract class BaseLayerBrush : PropertyChangedBase, IDisposable
    {
        private LayerBrushType _brushType;
        private LayerBrushConfigurationDialog _configurationDialog;
        private LayerBrushDescriptor _descriptor;
        private Layer _layer;
        private bool _supportsTransformation = true;

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
        public LayerBrushConfigurationDialog ConfigurationDialog
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
        ///     Gets the plugin info that defined this brush
        /// </summary>
        public PluginInfo PluginInfo => Descriptor.LayerBrushProvider.PluginInfo;

        /// <summary>
        ///     Gets a reference to the layer property group without knowing it's type
        /// </summary>
        public virtual LayerPropertyGroup BaseProperties => null;

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
                    throw new ArtemisPluginException(PluginInfo, "An RGB.NET brush cannot support transformation");
                _supportsTransformation = value;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            DisableLayerBrush();
            BaseProperties.Dispose();

            Dispose(true);
            GC.SuppressFinalize(this);
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

        // Not only is this needed to initialize properties on the layer brushes, it also prevents implementing anything
        // but LayerBrush<T> and RgbNetLayerBrush<T> outside the core
        internal abstract void Initialize(IRenderElementService renderElementService);

        internal abstract void InternalRender(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint);

        internal virtual void Dispose(bool disposing)
        {
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