namespace Artemis.Models
{
    public abstract class EffectSettings
    {
        /// <summary>
        ///     Loads the settings from the settings file
        /// </summary>
        public abstract void Load();

        /// <summary>
        ///     Saves the settings to the settings file
        /// </summary>
        public abstract void Save();

        /// <summary>
        ///     Returns the settings to their default value
        /// </summary>
        public abstract void ToDefault();
    }
}