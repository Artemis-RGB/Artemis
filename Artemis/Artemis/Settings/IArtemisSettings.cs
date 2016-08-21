namespace Artemis.Settings
{
    public interface IArtemisSettings
    {
        /// <summary>
        ///     Utility method for quickly saving this instance of settings.
        ///     Some settings might wrap logic around this
        /// </summary>
        void Save();

        /// <summary>
        ///     Resets this settings instance to its default values
        /// </summary>
        /// <param name="save">Save the settings after resetting them</param>
        void Reset(bool save = false);
    }
}