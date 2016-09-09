using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Process.NET;

namespace Artemis.Modules.Games.WoW.Data
{
    public class WoWObjectManager
    {
        public WoWObjectManager(IProcess process, IntPtr baseAddress)
        {
            Process = process;
            CurrentManagerAddress = process.Native.MainModule.BaseAddress + baseAddress.ToInt32();
        }

        public IProcess Process { get; set; }

        public IntPtr CurrentManagerAddress { get; set; }

        public Dictionary<WoWStructs.Guid, WoWObject> WoWObjects { get; set; }

        public IntPtr GetFirstObject()
        {
            var mgr = GetCurrentManager();
            return mgr.VisibleObjects.m_fulllist.baseClass.m_terminator.m_next;
        }

        public WoWStructs.CurrentManager GetCurrentManager()
        {
            return Process.Memory.Read<WoWStructs.CurrentManager>(Process.Memory.Read<IntPtr>(CurrentManagerAddress));
        }

        public IntPtr GetNextObjectFromCurrent(IntPtr current)
        {
            var mgr = GetCurrentManager();

            return Process.Memory.Read<IntPtr>(
                current + mgr.VisibleObjects.m_fulllist.baseClass.m_linkoffset + IntPtr.Size);
        }

        public void Update()
        {
            WoWObjects.Clear();
            var wowObjects = EnumVisibleObjects();
            foreach (var wowObject in wowObjects)
                WoWObjects[wowObject.Data.Guid] = wowObject;

            OnObjectsUpdated(WoWObjects);
        }

        public event EventHandler<Dictionary<WoWStructs.Guid, WoWObject>> ObjectsUpdated;

        // Loop through the games object list.
        public IEnumerable<WoWObject> EnumVisibleObjects()
        {
            var first = GetFirstObject();
            var typeOffset = Marshal.OffsetOf(typeof(WoWStructs.ObjectData), "ObjectType").ToInt32();

            while (((first.ToInt64() & 1) == 0) && (first != IntPtr.Zero))
            {
                var type = (WoWEnums.ObjectType) Process.Memory.Read<int>(first + typeOffset);

                // Fix below with other object types as added.
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (type)
                {
                    case WoWEnums.ObjectType.Object:
                        yield return new WoWObject(Process, first);
                        break;
                    case WoWEnums.ObjectType.Container:
                        break;
                    case WoWEnums.ObjectType.Unit:
                        yield return new WoWUnit(Process, first);
                        break;
                    case WoWEnums.ObjectType.Player:
                        yield return new WoWPlayer(Process, first, new IntPtr(0x179A6E0));
                        break;
                    default:
                        yield return new WoWObject(Process, first);
                        break;
                }

                first = GetNextObjectFromCurrent(first);
            }
        }

        protected virtual void OnObjectsUpdated(Dictionary<WoWStructs.Guid, WoWObject> e)
        {
            ObjectsUpdated?.Invoke(this, e);
        }
    }
}