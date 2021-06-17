using SkiaSharp;

namespace Artemis.Core.ScriptingProviders
{
    /// <summary>
    ///     Represents a script bound to a specific <see cref="Profile" /> processed by a <see cref="ScriptingProvider" />.
    /// </summary>
    public abstract class ProfileScript : Script
    {
        /// <summary>
        ///     Called whenever the profile is about to update
        /// </summary>
        /// <param name="deltaTime">Seconds passed since last update</param>
        public virtual void OnProfileUpdating(double deltaTime)
        {
        }

        /// <summary>
        ///     Called whenever the profile has been updated
        /// </summary>
        /// <param name="deltaTime">Seconds passed since last update</param>
        public virtual void OnProfileUpdated(double deltaTime)
        {
        }

        /// <summary>
        ///     Called whenever the profile is about to render
        /// </summary>
        /// <param name="canvas">The profile canvas</param>
        /// <param name="bounds">The area to be filled, covers the entire canvas</param>
        /// <param name="paint">The paint to be used to fill render the profile</param>
        public virtual void OnProfileRendering(SKCanvas canvas, SKRect bounds, SKPaint paint)
        {
        }

        /// <summary>
        ///     Called whenever the profile has been rendered
        /// </summary>
        /// <param name="canvas">The profile canvas</param>
        /// <param name="bounds">The area to be filled, covers the entire canvas</param>
        /// <param name="paint">The paint to be used to fill render the profile</param>
        public virtual void OnProfileRendered(SKCanvas canvas, SKRect bounds, SKPaint paint)
        {
        }
    }
}