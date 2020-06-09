using System.Collections.Generic;
using System.Collections.ObjectModel;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Plugins.LayerEffect;
using Artemis.Core.Plugins.LayerEffect.Abstract;

namespace Artemis.Core.Plugins.Abstract
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows you to create one or more <see cref="LayerEffect" />s usable by profile layers.
    /// </summary>
    public abstract class LayerEffectProvider : Plugin
    {
        private readonly List<LayerEffectDescriptor> _layerEffectDescriptors;

        protected LayerEffectProvider()
        {
            _layerEffectDescriptors = new List<LayerEffectDescriptor>();
        }

        /// <summary>
        ///     A read-only collection of all layer effects added with <see cref="AddLayerEffectDescriptor{T}" />
        /// </summary>
        public ReadOnlyCollection<LayerEffectDescriptor> LayerEffectDescriptors => _layerEffectDescriptors.AsReadOnly();

        /// <summary>
        ///     Adds a layer effect descriptor for a given layer effect, so that it appears in the UI.
        ///     <para>Note: You do not need to manually remove these on disable</para>
        /// </summary>
        /// <typeparam name="T">The type of the layer effect you wish to register</typeparam>
        /// <param name="displayName">The name to display in the UI</param>
        /// <param name="description">The description to display in the UI</param>
        /// <param name="icon">
        ///     The Material icon to display in the UI, a full reference can be found
        ///     <see href="https://materialdesignicons.com">here</see>
        /// </param>
        protected void AddLayerEffectDescriptor<T>(string displayName, string description, string icon) where T : BaseLayerEffect
        {
            if (!Enabled)
                throw new ArtemisPluginException(PluginInfo, "Can only add a layer effect descriptor when the plugin is enabled");

            _layerEffectDescriptors.Add(new LayerEffectDescriptor(displayName, description, icon, typeof(T), this));
        }
    }
}