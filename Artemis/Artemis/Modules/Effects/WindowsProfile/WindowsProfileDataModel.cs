using Artemis.Models.Interfaces;

namespace Artemis.Modules.Effects.WindowsProfile
{
    public class WindowsProfileDataModel : IDataModel
    {
        public CpuDataModel Cpu { get; set; }

        public WindowsProfileDataModel()
        {
            Cpu = new CpuDataModel();
        }
    }

    public class CpuDataModel
    {
        public int Core1Usage { get; set; }
        public int Core2Usage { get; set; }
        public int Core3Usage { get; set; }
        public int Core4Usage { get; set; }
        public int Core5Usage { get; set; }
        public int Core6Usage { get; set; }
        public int Core7Usage { get; set; }
        public int Core8Usage { get; set; }
    }
}