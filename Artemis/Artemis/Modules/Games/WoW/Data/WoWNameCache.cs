using System;
using System.Text;
using Process.NET;

namespace Artemis.Modules.Games.WoW.Data
{
    public class WoWNameCache
    {
        public WoWNameCache(ProcessSharp process, IntPtr baseAddress)
        {
            Process = process;
            CurrentCacheAddress = process.Native.MainModule.BaseAddress + baseAddress.ToInt32();
        }

        public ProcessSharp Process { get; set; }

        public IntPtr CurrentCacheAddress { get; set; }

        public WoWDetails GetNameByGuid(Guid searchGuid)
        {
            var current = Process.Memory.Read<IntPtr>(CurrentCacheAddress);
            var index = 0;
            while (current != IntPtr.Zero)
            {
                var guid = Process.Memory.Read<Guid>(current + 0x20);
                if (guid.Equals(searchGuid))
                {
                    var pRace = Process.Memory.Read<int>(current + 0x88);
                    var pClass = Process.Memory.Read<int>(current + 0x90);
                    var pName = Process.Memory.Read(current + 0x31, Encoding.ASCII, 48);

                    var name = new WoWDetails(guid, pRace, pClass, WoWEnums.WoWType.Player, pName);
                    return name;
                }

                if (index > 20000)
                    return null;

                index++;
                current = Process.Memory.Read<IntPtr>(current);
            }
            return null;
        }
    }

    public class WoWDetails
    {
        public WoWDetails(Guid guid, int race, int @class, WoWEnums.WoWType type, string name)
        {
            Guid = guid;
            Race = (WoWEnums.WoWRace) race;
            Class = (WoWEnums.WoWClass) @class;
            Type = type;
            Name = name;
        }

        public Guid Guid { get; set; }
        public WoWEnums.WoWRace Race { get; set; }
        public WoWEnums.WoWClass Class { get; set; }
        public WoWEnums.WoWType Type { get; set; }
        public string Name { get; set; }
    }
}