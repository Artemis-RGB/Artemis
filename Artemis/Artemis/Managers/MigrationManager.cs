using System.IO;
using Artemis.DAL;

namespace Artemis.Managers
{
    public class MigrationManager
    {
        private readonly DeviceManager _deviceManager;

        public MigrationManager(DeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
        }

        /// <summary>
        ///     Migrates old versions of profiles to new versions
        /// </summary>
        public void MigrateProfiles()
        {
            // 1.8.0.0 - Rename WindowsProfile to GeneralProfile
            foreach (var keyboardProvider in _deviceManager.KeyboardProviders)
            {
                var folder = ProfileProvider.ProfileFolder + "/" + keyboardProvider.Slug + "/WindowsProfile";
                if (!Directory.Exists(folder))
                    continue;

                // Get all the profiles
                var profiles = ProfileProvider.ReadProfiles(keyboardProvider.Slug + "/WindowsProfile");
                foreach (var profile in profiles)
                {
                    // Change their GameName and save, effectively moving them to the new folder
                    profile.GameName = "GeneralProfile";
                    ProfileProvider.AddOrUpdate(profile);
                }
                // Delete the old profiles
                Directory.Delete(folder, true);
            }
        }
    }
}