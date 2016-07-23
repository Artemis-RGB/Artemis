namespace Artemis.Modules.Games.WorldofWarcraft
{
    public static class WoWAddresses
    {
        public enum ActivateSettings
        {
            Activate_Offset = 0x34,
            AutoDismount_Activate_Pointer = 0xe56850,
            AutoInteract_Activate_Pointer = 0xe56848,
            AutoLoot_Activate_Pointer = 0xe56868,
            AutoSelfCast_Activate_Pointer = 0xe56874
        }

        public enum Battleground
        {
            MaxBattlegroundId = 0xec3fdc,
            PvpExitWindow = 0xec4198,
            StatPvp = 0xc3c03c
        }

        public enum Chat
        {
            chatBufferPos = 0xeb1bf0,
            chatBufferStart = 0xe58190,
            msgFormatedChat = 0x65,
            NextMessage = 0x17e8
        }

        public enum ClickToMove
        {
            CTM = 0xddf8f0,
            CTM_PUSH = 0xddf8ac,
            CTM_X = 0xddf918,
            CTM_Y = 0xddf91c,
            CTM_Z = 0xddf920
        }

        public enum CorpsePlayer
        {
            X = 0xe57894,
            Y = 0xe57898,
            Z = 0xe5789c
        }

        public enum DBC
        {
            FactionTemplate = 0,
            ItemClass = 0xd173c0,
            ItemSubClass = 0,
            Lock = 0,
            Map = 0xd291a0,
            QuestPOIPoint = 0xd1e950,
            ResearchSite = 0xd1d2d0,
            SpellCategories = 0,
            Unknown = 0xf35428
        }

        public enum EventsListener
        {
            BaseEvents = 0xcb2474,
            EventOffsetCount = 0x48,
            EventOffsetName = 0x18,
            EventsCount = 0xcb2470
        }

        public enum Fishing
        {
            BobberHasMoved = 0xf8
        }

        public enum FunctionWow
        {
            CGUnit_C__InitializeTrackingState = 0x30623b,
            CGUnit_C__Interact = 0x524ff,
            CGWorldFrame__Intersect = 0x5e46ab,
            ClntObjMgrGetActivePlayerObj = 0x816d7,
            FrameScript__GetLocalizedText = 0x300b48,
            FrameScript_ExecuteBuffer = 0xa6772,
            IsOutdoors = 0,
            Spell_C_HandleTerrainClick = 0x2b76ff,
            strlen = 0x74fcb0,
            UnitCanAttack = 0,
            WowClientDB2__GetRowPointer = 0x20c775
        }

        public enum GameInfo
        {
            AreaId = 0xc32c2c,
            buildWoWVersionString = 0xd002a8,
            gameState = 0xe56a49,
            GetTime = 0xcb2150,
            isLoading = 0xca59b0,
            LastHardwareAction = 0xd0e090,
            MapTextureId = 0xc3bd28,
            SubAreaId = 0xc32c24,
            subZoneMap = 0xe56a68,
            TextBoxActivated = 0xbbe9ac,
            zoneMap = 0xe56a64
        }

        public enum GameObject
        {
            CachedCastBarCaption = 12,
            CachedData0 = 20,
            CachedIconName = 8,
            CachedName = 180,
            CachedQuestItem1 = 0x9c,
            CachedSize = 0x98,
            DBCacheRow = 620,
            GAMEOBJECT_FIELD_X = 0x138,
            GAMEOBJECT_FIELD_Y = 0x13c,
            GAMEOBJECT_FIELD_Z = 320,
            PackedRotationQuaternion = 0x150,
            TransformationMatrice = 0x278
        }

        public enum Hooking
        {
            DX_DEVICE = 0xcc523c,
            DX_DEVICE_IDX = 0x2508,
            ENDSCENE_IDX = 0xa8
        }

        public enum Login
        {
            realmName = 0xf35e16
        }

        public enum MovementFlagsOffsets
        {
            Offset1 = 0x124,
            Offset2 = 0x40
        }

        public enum ObjectManager
        {
            continentId = 0x108,
            firstObject = 0xd8,
            localGuid = 0xf8,
            nextObject = 0x44,
            objectGUID = 0x30,
            objectTYPE = 0x10
        }

        public class ObjectManagerClass
        {
            public static uint clientConnection;
            public static uint sCurMgr;
        }

        public enum Party
        {
            NumOfPlayers = 200,
            NumOfPlayersSuBGroup = 0xcc,
            PartyOffset = 0xeb5458,
            PlayerGuid = 0x10
        }

        public enum PetBattle
        {
            IsInBattle = 0xba8a10
        }

        public enum Player
        {
            LocalPlayerSpellsOnCooldown = 0xd372b8,
            petGUID = 0xec7158,
            playerName = 0xf35e20,
            RetrieveCorpseWindow = 0xe576f4,
            RuneStartCooldown = 0xf18aa8,
            SkillMaxValue = 0x400,
            SkillValue = 0x200
        }

        public enum PlayerNameStore
        {
            PlayerNameNextOffset = 20,
            PlayerNameStorePtr = 0xd0b4e0,
            PlayerNameStringOffset = 0x11
        }

        public enum PowerIndex
        {
            Multiplicator = 0x10,
            PowerIndexArrays = 0xddf914
        }

        public enum Quests
        {
            QuestGiverStatus = 0xf4
        }

        public enum SpellBook
        {
            FirstTalentBookPtr = 0xeb52ec,
            KnownAllSpells = 0xeb5130,
            MountBookMountsPtr = 0xeb5194,
            MountBookNumMounts = 0xeb5190,
            NextTalentBookPtr = 0xeb52e4,
            SpellBookNumSpells = 0xeb5134,
            SpellBookSpellsPtr = 0xeb5138,
            SpellDBCMaxIndex = 0x30d40,
            TalentBookOverrideSpellId = 0x1c,
            TalentBookSpellId = 20
        }

        public enum UnitBaseGetUnitAura
        {
            AuraSize = 0x58,
            AuraStructCasterLevel = 0x3a,
            AuraStructCount = 0x39,
            AuraStructCreatorGuid = 0x20,
            AuraStructDuration = 60,
            AuraStructFlag = 0x34,
            AuraStructMask = 0x35,
            AuraStructSpellEndTime = 0x40,
            AuraStructSpellId = 0x30,
            AuraStructUnk1 = 0x3b,
            AuraStructUnk2 = 0x44,
            AuraTable1 = 0x1150,
            AuraTable2 = 0x580
        }

        public enum UnitField
        {
            CachedIsBoss = 0x60,
            CachedModelId1 = 0x6c,
            CachedName = 0x80,
            CachedQuestItem1 = 60,
            CachedSubName = 0,
            CachedTypeFlag = 0x24,
            CachedUnitClassification = 0x2c,
            CanInterrupt = 0xfc4,
            CanInterruptOffset = 0xe02ea0,
            CanInterruptOffset2 = 0xe02ea4,
            CanInterruptOffset3 = 0xe02ea8,
            CastingSpellEndTime = 0x108c,
            CastingSpellID = 0x1064,
            CastingSpellStartTime = 0x1088,
            ChannelSpellEndTime = 0x1098,
            ChannelSpellID = 0x1090,
            ChannelSpellStartTime = 0x1094,
            DBCacheRow = 0xc80,
            TransportGUID = 0xae8,
            UNIT_FIELD_R = 0xb08,
            UNIT_FIELD_X = 0xaf8,
            UNIT_FIELD_Y = 0xafc,
            UNIT_FIELD_Z = 0xb00
        }

        public enum VMT
        {
            CGUnit_C__GetFacing = 0x35
        }
    }
}