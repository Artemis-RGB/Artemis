using System;
using System.IO;
using Artemis.Core;
using Artemis.UI.Shared.Controls;
using MaterialDesignThemes.Wpf;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Provides utilities for UI-related plugin tasks
    /// </summary>
    public static class PluginUtilities
    {
        /// <summary>
        ///     Transforms the provided icon so that it is usable by the <see cref="ArtemisIcon" /> control
        /// </summary>
        /// <param name="pluginInfo">The info of the plugin the icon belongs to</param>
        /// <param name="icon">
        ///     The icon, may be a string representation of a <see cref="PackIconKind" /> or a relative path
        ///     pointing to a .svg file
        /// </param>
        /// <returns></returns>
        public static object GetPluginIcon(PluginInfo pluginInfo, string icon)
        {
            if (icon == null)
                return PackIconKind.QuestionMarkCircle;

            // Icon is provided as a path
            if (icon.EndsWith(".svg"))
            {
                string iconPath = pluginInfo.ResolveRelativePath(icon);
                if (!File.Exists(iconPath))
                    return PackIconKind.QuestionMarkCircle;
                return iconPath;
            }

            // Icon is provided as string to avoid having to reference MaterialDesignThemes
            bool parsedIcon = Enum.TryParse(icon, true, out PackIconKind iconEnum);
            if (parsedIcon == false)
                iconEnum = PackIconKind.QuestionMarkCircle;
            return iconEnum;

            return icon;
        }
    }
}