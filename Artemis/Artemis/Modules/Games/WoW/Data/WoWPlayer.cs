using System;
using System.Linq;
using Process.NET;

namespace Artemis.Modules.Games.WoW.Data
{
    public class WoWPlayer : WoWUnit
    {
        private readonly IntPtr _targetIntPtr;

        public WoWPlayer(IProcess process, IntPtr baseAddress, IntPtr targetIntPtr, bool readPointer = false)
            : base(process, baseAddress, readPointer)
        {
            _targetIntPtr = targetIntPtr;
        }

        public WoWObject GetTarget(WoWObjectManager manager)
        {
            var targetGuid = Process.Memory.Read<Guid>(Process.Native.MainModule.BaseAddress + _targetIntPtr.ToInt32());
            return manager.EnumVisibleObjects().FirstOrDefault(o => o.Guid == targetGuid);
        }
    }
}