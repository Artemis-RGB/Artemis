using Artemis.Models;

namespace Artemis.Modules.Effects.WindowsProfile
{
    public class WindowsProfileSettings : GameSettings
    {
        public WindowsProfileSettings()
        {
            Load();
        }

        public sealed override void Load()
        {
            LastProfile = WindowsProfile.Default.LastProfile;
        }

        public sealed override void Save()
        {
            WindowsProfile.Default.LastProfile = LastProfile;

            WindowsProfile.Default.Save();
        }

        public sealed override void ToDefault()
        {
        }
    }
}