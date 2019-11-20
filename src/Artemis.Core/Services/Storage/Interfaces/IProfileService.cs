using System.Collections.Generic;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services.Interfaces;

namespace Artemis.Core.Services.Storage.Interfaces
{
    public interface IProfileService : IArtemisService
    {
        List<Profile> GetProfiles(ProfileModule module);
        Profile GetActiveProfile(ProfileModule module);
    }
}