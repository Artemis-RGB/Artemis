namespace Artemis.Core
{
    /// <summary>
    ///     An optional entry point for your plugin
    /// </summary>
    public abstract class PluginBootstrapper
    {
        private Plugin? _plugin;

        /// <summary>
        ///     Called when the plugin is loaded
        /// </summary>
        /// <param name="plugin"></param>
        public virtual void OnPluginLoaded(Plugin plugin)
        {
        }

        /// <summary>
        ///     Called when the plugin is activated
        /// </summary>
        /// <param name="plugin">The plugin instance of your plugin</param>
        public virtual void OnPluginEnabled(Plugin plugin)
        {
        }

        /// <summary>
        ///     Called when the plugin is deactivated or when Artemis shuts down
        /// </summary>
        /// <param name="plugin">The plugin instance of your plugin</param>
        public virtual void OnPluginDisabled(Plugin plugin)
        {
        }

        /// <summary>
        ///     Adds the provided prerequisite to the plugin.
        /// </summary>
        /// <param name="prerequisite">The prerequisite to add</param>
        public void AddPluginPrerequisite(PluginPrerequisite prerequisite)
        {
            // TODO: We can keep track of them and add them after load, same goes for the others
            if (_plugin == null)
                throw new ArtemisPluginException("Cannot add plugin prerequisites before the plugin is loaded");

            if (!_plugin.Info.Prerequisites.Contains(prerequisite))
                _plugin.Info.Prerequisites.Add(prerequisite);
        }

        /// <summary>
        ///     Removes the provided prerequisite from the plugin.
        /// </summary>
        /// <param name="prerequisite">The prerequisite to remove</param>
        /// <returns>
        ///     <see langword="true" /> is successfully removed; otherwise <see langword="false" />. This method also returns
        ///     <see langword="false" /> if the prerequisite was not found.
        /// </returns>
        public bool RemovePluginPrerequisite(PluginPrerequisite prerequisite)
        {
            if (_plugin == null)
                throw new ArtemisPluginException("Cannot add plugin prerequisites before the plugin is loaded");

            return _plugin.Info.Prerequisites.Remove(prerequisite);
        }

        /// <summary>
        ///     Adds the provided prerequisite to the feature of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="prerequisite">The prerequisite to add</param>
        public void AddFeaturePrerequisite<T>(PluginPrerequisite prerequisite) where T : PluginFeature
        {
            if (_plugin == null)
                throw new ArtemisPluginException("Cannot add feature prerequisites before the plugin is loaded");

            PluginFeatureInfo info = _plugin.GetFeatureInfo<T>();
            if (!info.Prerequisites.Contains(prerequisite))
                info.Prerequisites.Add(prerequisite);
        }

        /// <summary>
        ///     Removes the provided prerequisite from the feature of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="prerequisite">The prerequisite to remove</param>
        /// <returns>
        ///     <see langword="true" /> is successfully removed; otherwise <see langword="false" />. This method also returns
        ///     <see langword="false" /> if the prerequisite was not found.
        /// </returns>
        public bool RemoveFeaturePrerequisite<T>(PluginPrerequisite prerequisite) where T : PluginFeature
        {
            if (_plugin == null)
                throw new ArtemisPluginException("Cannot add feature prerequisites before the plugin is loaded");

            return _plugin.GetFeatureInfo<T>().Prerequisites.Remove(prerequisite);
        }

        internal void InternalOnPluginLoaded(Plugin plugin)
        {
            _plugin = plugin;
            OnPluginLoaded(plugin);
        }
    }
}