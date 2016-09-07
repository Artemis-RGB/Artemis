using System;
using System.Runtime.InteropServices;
using Artemis.Modules.Games.WoW.Data.WowSharp.Client.Patchables;

namespace Artemis.Modules.Games.WoW.Data
{
    public static class WoWStructs
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct ObjectData
        {
            // x32 : x64
            [FieldOffset(0)] private readonly IntPtr vtable; // 0x00 0x00
            [FieldOffset(10)] public IntPtr Descriptors; // 0x04 0x10
            [FieldOffset(18)] private readonly IntPtr unk1; // 0x08 0x18
            [FieldOffset(20)] public int ObjectType; // 0x0C 0x20
            [FieldOffset(24)] private readonly IntPtr unk3; // 0x10 0x24
            [FieldOffset(28)] private readonly IntPtr unk4; // 0x14 0x28
            [FieldOffset(30)] private readonly IntPtr unk5; // 0x18 0x30
            [FieldOffset(38)] private readonly IntPtr unk6; // 0x1C 0x38
            [FieldOffset(40)] private readonly IntPtr unk7; // 0x20 0x40
            [FieldOffset(48)] private readonly IntPtr unk8; // 0x24 0x48
            [FieldOffset(50)] public Guid Guid; // 0x28 0x50
        }

        public struct Guid
        {
            private readonly Int128 _mGuid;

            public static readonly Guid Zero = new Guid(0);

            public Guid(Int128 guid)
            {
                _mGuid = guid;
            }

            public override string ToString()
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (Type)
                {
                    case WoWEnums.GuidType.Creature:
                    case WoWEnums.GuidType.Vehicle:
                    case WoWEnums.GuidType.Pet:
                    case WoWEnums.GuidType.GameObject:
                    case WoWEnums.GuidType.AreaTrigger:
                    case WoWEnums.GuidType.DynamicObject:
                    case WoWEnums.GuidType.Corpse:
                    case WoWEnums.GuidType.LootObject:
                    case WoWEnums.GuidType.SceneObject:
                    case WoWEnums.GuidType.Scenario:
                    case WoWEnums.GuidType.AiGroup:
                    case WoWEnums.GuidType.DynamicDoor:
                    case WoWEnums.GuidType.Vignette:
                    case WoWEnums.GuidType.Conversation:
                    case WoWEnums.GuidType.CallForHelp:
                    case WoWEnums.GuidType.AiResource:
                    case WoWEnums.GuidType.AiLock:
                    case WoWEnums.GuidType.AiLockTicket:
                        return $"{Type}-{SubType}-{RealmId}-{MapId}-{ServerId}-{Id}-{CreationBits:X10}";
                    case WoWEnums.GuidType.Player:
                        return $"{Type}-{RealmId}-{(ulong) (_mGuid >> 64):X8}";
                    case WoWEnums.GuidType.Item:
                        return $"{Type}-{RealmId}-{(uint) ((_mGuid >> 18) & 0xFFFFFF)}-{(ulong) (_mGuid >> 64):X10}";
                    //case GuidType.ClientActor:
                    //    return String.Format("{0}-{1}-{2}", Type, RealmId, CreationBits);
                    //case GuidType.Transport:
                    //case GuidType.StaticDoor:
                    //    return String.Format("{0}-{1}-{2}", Type, RealmId, CreationBits);
                    default:
                        return $"{Type}-{_mGuid:X32}";
                }
            }

            public override bool Equals(object obj)
            {
                if (obj is Guid)
                    return _mGuid == ((Guid) obj)._mGuid;
                return false;
            }

            public override int GetHashCode()
            {
                return _mGuid.GetHashCode();
            }

            public WoWEnums.GuidType Type => (WoWEnums.GuidType) (byte) ((_mGuid >> 58) & 0x3F);

            public byte SubType => (byte) ((_mGuid >> 120) & 0x3F);

            public ushort RealmId => (ushort) ((_mGuid >> 42) & 0x1FFF);

            public ushort ServerId => (ushort) ((_mGuid >> 104) & 0x1FFF);

            public ushort MapId => (ushort) ((_mGuid >> 29) & 0x1FFF);

            // Creature, Pet, Vehicle
            public uint Id => (uint) ((_mGuid >> 6) & 0x7FFFFF);

            public ulong CreationBits => (ulong) ((_mGuid >> 64) & 0xFFFFFFFFFF);
        }

        #region Manager

        // Region is here due to the large amount of structs-
        // the CurremtManager struct depends on.
        [StructLayout(LayoutKind.Sequential)]
        public struct CurrentManager
        {
            public TsHashTable VisibleObjects; // m_objects
            public TsHashTable LazyCleanupObjects; // m_lazyCleanupObjects

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
            // m_lazyCleanupFifo, m_freeObjects, m_visibleObjects, m_reenabledObjects, whateverObjects...
            public TsExplicitList[] Links; // Links[13] has all objects stored in VisibleObjects it seems

#if !X64
            public int Unknown1; // wtf is that and why x86 only?
#endif
            public Int128 ActivePlayer;
            public int MapId;
            public IntPtr ClientConnection;
            public IntPtr MovementGlobals;
            public int Unk1;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Ts
        {
            public IntPtr vtable;
            public uint m_alloc;
            public uint m_count;
            public IntPtr m_data; //TSExplicitList* m_data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TsExplicitList
        {
            public TsList baseClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TsFixedArray
        {
            public Ts baseClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TsGrowableArray
        {
            public TsFixedArray baseclass;
            public uint m_chunk;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TsHashTable
        {
            public IntPtr vtable;
            public TsExplicitList m_fulllist;
            public int m_fullnessIndicator;
            public TsGrowableArray m_slotlistarray;
            public int m_slotmask;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TsLink
        {
            public IntPtr m_prevlink; //TSLink *m_prevlink
            public IntPtr m_next; // C_OBJECTHASH *m_next
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TsList
        {
            public int m_linkoffset;
            public TsLink m_terminator;
        }

        #endregion;
    }
}