using System;
using System.Text;
using Newtonsoft.Json;
using Process.NET;

namespace Artemis.Modules.Games.WoW.Data
{
    public class WoWObject
    {
        private readonly bool _readPointer;

        public WoWObject(IProcess process, IntPtr baseAddress, bool readPointer = false)
        {
            Process = process;
            BaseAddress = baseAddress;
            _readPointer = readPointer;

            Guid = ReadField<Guid>(0x00);
        }

        [JsonIgnore]
        public IntPtr BaseAddress { get; set; }

        [JsonIgnore]
        public IProcess Process { get; set; }

        public Guid Guid { get; set; }

        [JsonIgnore]
        public WoWStructs.ObjectData Data { get; set; }

        public T ReadField<T>(int offset)
        {
            var address = GetAddress();
            if (address == IntPtr.Zero)
                return default(T);

            var ptr = Process.Memory.Read<IntPtr>(address + 0x10);
            return Process.Memory.Read<T>(ptr + offset);
        }

        public T ReadField<T>(Enum offset)
        {
            var address = GetAddress();
            if (address == IntPtr.Zero)
                return default(T);

            var ptr = Process.Memory.Read<IntPtr>(address + 0x10);
            return Process.Memory.Read<T>(ptr + Convert.ToInt32(offset));
        }

        private IntPtr GetAddress()
        {
            return _readPointer
                ? Process.Memory.Read<IntPtr>(Process.Native.MainModule.BaseAddress + BaseAddress.ToInt32())
                : BaseAddress;
        }

        public WoWDetails GetNpcDetails()
        {
            var address = GetAddress();
            if (address == IntPtr.Zero)
                return null;

            var npcCachePtr = Process.Memory.Read<IntPtr>(address + 0x1760);
            if (npcCachePtr == IntPtr.Zero)
                return null;

            var npcName = Process.Memory.Read(Process.Memory.Read<IntPtr>(npcCachePtr + 0x00A0), Encoding.ASCII, 48);
            return new WoWDetails(Guid, 0, 0, WoWEnums.WoWType.Npc, npcName);
        }
    }
}