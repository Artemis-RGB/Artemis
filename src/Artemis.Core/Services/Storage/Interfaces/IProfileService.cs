using System.Collections.Generic;
using System.Threading.Tasks;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services.Interfaces;

namespace Artemis.Core.Services.Storage.Interfaces
{
    public interface IProfileService : IArtemisService
    {
        Task<ICollection<Profile>> GetProfiles(ProfileModule module);
        Task<Profile> GetActiveProfile(ProfileModule module);
    }
}