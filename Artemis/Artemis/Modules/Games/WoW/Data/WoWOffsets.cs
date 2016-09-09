namespace Artemis.Modules.Games.WoW.Data
{
    internal static class WoWOffsets
    {
        internal enum AreaTriggerData
        {
            OverrideScaleCurve = 0x30, // Size: 0x7, Flags: 0x201
            ExtraScaleCurve = 0x4C, // Size: 0x7, Flags: 0x201
            Caster = 0x68, // Size: 0x4, Flags: 0x1
            Duration = 0x78, // Size: 0x1, Flags: 0x1
            TimeToTarget = 0x7C, // Size: 0x1, Flags: 0x201
            TimeToTargetScale = 0x80, // Size: 0x1, Flags: 0x201
            TimeToTargetExtraScale = 0x84, // Size: 0x1, Flags: 0x201
            SpellId = 0x88, // Size: 0x1, Flags: 0x1
            SpellVisualId = 0x8C, // Size: 0x1, Flags: 0x80
            BoundsRadius2D = 0x90, // Size: 0x1, Flags: 0x280
            DecalPropertiesId = 0x94 // Size: 0x1, Flags: 0x1
        }

        internal enum ContainerData
        {
            Slots = 0x150, // Size: 0x90, Flags: 0x1
            NumSlots = 0x390 // Size: 0x1, Flags: 0x1
        }

        internal enum ConversationData
        {
            LastLineDuration = 0x30 // Size: 0x1, Flags: 0x80
        }

        internal enum CorpseData
        {
            Owner = 0x30, // Size: 0x4, Flags: 0x1
            PartyGuid = 0x40, // Size: 0x4, Flags: 0x1
            DisplayId = 0x50, // Size: 0x1, Flags: 0x1
            Items = 0x54, // Size: 0x13, Flags: 0x1
            SkinId = 0xA0, // Size: 0x1, Flags: 0x1
            FacialHairStyleId = 0xA4, // Size: 0x1, Flags: 0x1
            Flags = 0xA8, // Size: 0x1, Flags: 0x1
            DynamicFlags = 0xAC, // Size: 0x1, Flags: 0x80
            FactionTemplate = 0xB0, // Size: 0x1, Flags: 0x1
            CustomDisplayOption = 0xB4 // Size: 0x1, Flags: 0x1
        }

        internal enum DynamicObjectData
        {
            Caster = 0x30, // Size: 0x4, Flags: 0x1
            TypeAndVisualId = 0x40, // Size: 0x1, Flags: 0x80
            SpellId = 0x44, // Size: 0x1, Flags: 0x1
            Radius = 0x48, // Size: 0x1, Flags: 0x1
            CastTime = 0x4C // Size: 0x1, Flags: 0x1
        }

        internal enum GameObjectData
        {
            CreatedBy = 0x30, // Size: 0x4, Flags: 0x1
            DisplayId = 0x40, // Size: 0x1, Flags: 0x280
            Flags = 0x44, // Size: 0x1, Flags: 0x201
            ParentRotation = 0x48, // Size: 0x4, Flags: 0x1
            FactionTemplate = 0x58, // Size: 0x1, Flags: 0x1
            Level = 0x5C, // Size: 0x1, Flags: 0x1
            PercentHealth = 0x60, // Size: 0x1, Flags: 0x201
            SpellVisualId = 0x64, // Size: 0x1, Flags: 0x281
            StateSpellVisualId = 0x68, // Size: 0x1, Flags: 0x280
            StateAnimId = 0x6C, // Size: 0x1, Flags: 0x280
            StateAnimKitId = 0x70, // Size: 0x1, Flags: 0x280
            StateWorldEffectId = 0x74 // Size: 0x4, Flags: 0x280
        }

        internal enum ItemData
        {
            Owner = 0x30, // Size: 0x4, Flags: 0x1
            ContainedIn = 0x40, // Size: 0x4, Flags: 0x1
            Creator = 0x50, // Size: 0x4, Flags: 0x1
            GiftCreator = 0x60, // Size: 0x4, Flags: 0x1
            StackCount = 0x70, // Size: 0x1, Flags: 0x4
            Expiration = 0x74, // Size: 0x1, Flags: 0x4
            SpellCharges = 0x78, // Size: 0x5, Flags: 0x4
            DynamicFlags = 0x8C, // Size: 0x1, Flags: 0x1
            Enchantment = 0x90, // Size: 0x27, Flags: 0x1
            PropertySeed = 0x12C, // Size: 0x1, Flags: 0x1
            RandomPropertiesId = 0x130, // Size: 0x1, Flags: 0x1
            Durability = 0x134, // Size: 0x1, Flags: 0x4
            MaxDurability = 0x138, // Size: 0x1, Flags: 0x4
            CreatePlayedTime = 0x13C, // Size: 0x1, Flags: 0x1
            ModifiersMask = 0x140, // Size: 0x1, Flags: 0x4
            Context = 0x144, // Size: 0x1, Flags: 0x1
            ArtifactXp = 0x148, // Size: 0x1, Flags: 0x4
            ItemAppearanceModId = 0x14C // Size: 0x1, Flags: 0x4
        }

        internal enum KeyBinding
        {
            NumKeyBindings = 0x1700030, // -0x17C0
            First = 0xC8,
            Next = 0xB8,
            Key = 0x30,
            Command = 0x58
        }

        internal enum ObjectData
        {
            Guid = 0x0, // Size: 0x4, Flags: 0x1
            Data = 0x10, // Size: 0x4, Flags: 0x1
            Type = 0x20, // Size: 0x1, Flags: 0x1
            EntryId = 0x24, // Size: 0x1, Flags: 0x80
            DynamicFlags = 0x28, // Size: 0x1, Flags: 0x280
            Scale = 0x2C // Size: 0x1, Flags: 0x1
        }

        internal enum PlayerData
        {
            DuelArbiter = 0x360, // Size: 0x4, Flags: 0x1
            WowAccount = 0x370, // Size: 0x4, Flags: 0x1
            LootTargetGuid = 0x380, // Size: 0x4, Flags: 0x1
            PlayerFlags = 0x390, // Size: 0x1, Flags: 0x1
            PlayerFlagsEx = 0x394, // Size: 0x1, Flags: 0x1
            GuildRankId = 0x398, // Size: 0x1, Flags: 0x1
            GuildDeleteDate = 0x39C, // Size: 0x1, Flags: 0x1
            GuildLevel = 0x3A0, // Size: 0x1, Flags: 0x1
            HairColorId = 0x3A4, // Size: 0x1, Flags: 0x1
            CustomDisplayOption = 0x3A8, // Size: 0x1, Flags: 0x1
            Inebriation = 0x3AC, // Size: 0x1, Flags: 0x1
            ArenaFaction = 0x3B0, // Size: 0x1, Flags: 0x1
            DuelTeam = 0x3B4, // Size: 0x1, Flags: 0x1
            GuildTimeStamp = 0x3B8, // Size: 0x1, Flags: 0x1
            QuestLog = 0x3BC, // Size: 0x320, Flags: 0x20
            VisibleItems = 0x103C, // Size: 0x26, Flags: 0x1
            PlayerTitle = 0x10D4, // Size: 0x1, Flags: 0x1
            FakeInebriation = 0x10D8, // Size: 0x1, Flags: 0x1
            VirtualPlayerRealm = 0x10DC, // Size: 0x1, Flags: 0x1
            CurrentSpecId = 0x10E0, // Size: 0x1, Flags: 0x1
            TaxiMountAnimKitId = 0x10E4, // Size: 0x1, Flags: 0x1
            AvgItemLevel = 0x10E8, // Size: 0x4, Flags: 0x1
            CurrentBattlePetBreedQuality = 0x10F8, // Size: 0x1, Flags: 0x1
            Prestige = 0x10FC, // Size: 0x1, Flags: 0x1
            HonorLevel = 0x1100, // Size: 0x1, Flags: 0x1
            InvSlots = 0x1104, // Size: 0x2EC, Flags: 0x2
            FarsightObject = 0x1CB4, // Size: 0x4, Flags: 0x2
            SummonedBattlePetGuid = 0x1CC4, // Size: 0x4, Flags: 0x2
            KnownTitles = 0x1CD4, // Size: 0xC, Flags: 0x2
            Coinage = 0x1D04, // Size: 0x2, Flags: 0x2
            Xp = 0x1D0C, // Size: 0x1, Flags: 0x2
            NextLevelXp = 0x1D10, // Size: 0x1, Flags: 0x2
            Skill = 0x1D14, // Size: 0x1C0, Flags: 0x2
            CharacterPoints = 0x2414, // Size: 0x1, Flags: 0x2
            MaxTalentTiers = 0x2418, // Size: 0x1, Flags: 0x2
            TrackCreatureMask = 0x241C, // Size: 0x1, Flags: 0x2
            TrackResourceMask = 0x2420, // Size: 0x1, Flags: 0x2
            MainhandExpertise = 0x2424, // Size: 0x1, Flags: 0x2
            OffhandExpertise = 0x2428, // Size: 0x1, Flags: 0x2
            RangedExpertise = 0x242C, // Size: 0x1, Flags: 0x2
            CombatRatingExpertise = 0x2430, // Size: 0x1, Flags: 0x2
            BlockPercentage = 0x2434, // Size: 0x1, Flags: 0x2
            DodgePercentage = 0x2438, // Size: 0x1, Flags: 0x2
            ParryPercentage = 0x243C, // Size: 0x1, Flags: 0x2
            CritPercentage = 0x2440, // Size: 0x1, Flags: 0x2
            RangedCritPercentage = 0x2444, // Size: 0x1, Flags: 0x2
            OffhandCritPercentage = 0x2448, // Size: 0x1, Flags: 0x2
            SpellCritPercentage = 0x244C, // Size: 0x1, Flags: 0x2
            ShieldBlock = 0x2450, // Size: 0x1, Flags: 0x2
            ShieldBlockCritPercentage = 0x2454, // Size: 0x1, Flags: 0x2
            Mastery = 0x2458, // Size: 0x1, Flags: 0x2
            Speed = 0x245C, // Size: 0x1, Flags: 0x2
            Lifesteal = 0x2460, // Size: 0x1, Flags: 0x2
            Avoidance = 0x2464, // Size: 0x1, Flags: 0x2
            Sturdiness = 0x2468, // Size: 0x1, Flags: 0x2
            Versatility = 0x246C, // Size: 0x1, Flags: 0x2
            VersatilityBonus = 0x2470, // Size: 0x1, Flags: 0x2
            PvpPowerDamage = 0x2474, // Size: 0x1, Flags: 0x2
            PvpPowerHealing = 0x2478, // Size: 0x1, Flags: 0x2
            ExploredZones = 0x247C, // Size: 0x100, Flags: 0x2
            RestInfo = 0x287C, // Size: 0x4, Flags: 0x2
            ModDamageDonePos = 0x288C, // Size: 0x7, Flags: 0x2
            ModDamageDoneNeg = 0x28A8, // Size: 0x7, Flags: 0x2
            ModDamageDonePercent = 0x28C4, // Size: 0x7, Flags: 0x2
            ModHealingDonePos = 0x28E0, // Size: 0x1, Flags: 0x2
            ModHealingPercent = 0x28E4, // Size: 0x1, Flags: 0x2
            ModHealingDonePercent = 0x28E8, // Size: 0x1, Flags: 0x2
            ModPeriodicHealingDonePercent = 0x28EC, // Size: 0x1, Flags: 0x2
            WeaponDmgMultipliers = 0x28F0, // Size: 0x3, Flags: 0x2
            WeaponAtkSpeedMultipliers = 0x28FC, // Size: 0x3, Flags: 0x2
            ModSpellPowerPercent = 0x2908, // Size: 0x1, Flags: 0x2
            ModResiliencePercent = 0x290C, // Size: 0x1, Flags: 0x2
            OverrideSpellPowerByApPercent = 0x2910, // Size: 0x1, Flags: 0x2
            OverrideApBySpellPowerPercent = 0x2914, // Size: 0x1, Flags: 0x2
            ModTargetResistance = 0x2918, // Size: 0x1, Flags: 0x2
            ModTargetPhysicalResistance = 0x291C, // Size: 0x1, Flags: 0x2
            LocalFlags = 0x2920, // Size: 0x1, Flags: 0x2
            NumRespecs = 0x2924, // Size: 0x1, Flags: 0x2
            SelfResSpell = 0x2928, // Size: 0x1, Flags: 0x2
            PvpMedals = 0x292C, // Size: 0x1, Flags: 0x2
            BuybackPrice = 0x2930, // Size: 0xC, Flags: 0x2
            BuybackTimestamp = 0x2960, // Size: 0xC, Flags: 0x2
            YesterdayHonorableKills = 0x2990, // Size: 0x1, Flags: 0x2
            LifetimeHonorableKills = 0x2994, // Size: 0x1, Flags: 0x2
            WatchedFactionIndex = 0x2998, // Size: 0x1, Flags: 0x2
            CombatRatings = 0x299C, // Size: 0x20, Flags: 0x2
            PvpInfo = 0x2A1C, // Size: 0x24, Flags: 0x2
            MaxLevel = 0x2AAC, // Size: 0x1, Flags: 0x2
            ScalingPlayerLevelDelta = 0x2AB0, // Size: 0x1, Flags: 0x2
            MaxCreatureScalingLevel = 0x2AB4, // Size: 0x1, Flags: 0x2
            NoReagentCostMask = 0x2AB8, // Size: 0x4, Flags: 0x2
            PetSpellPower = 0x2AC8, // Size: 0x1, Flags: 0x2
            Researching = 0x2ACC, // Size: 0xA, Flags: 0x2
            ProfessionSkillLine = 0x2AF4, // Size: 0x2, Flags: 0x2
            UiHitModifier = 0x2AFC, // Size: 0x1, Flags: 0x2
            UiSpellHitModifier = 0x2B00, // Size: 0x1, Flags: 0x2
            HomeRealmTimeOffset = 0x2B04, // Size: 0x1, Flags: 0x2
            ModPetHaste = 0x2B08, // Size: 0x1, Flags: 0x2
            OverrideSpellsId = 0x2B0C, // Size: 0x1, Flags: 0x402
            LfgBonusFactionId = 0x2B10, // Size: 0x1, Flags: 0x2
            LootSpecId = 0x2B14, // Size: 0x1, Flags: 0x2
            OverrideZonePvpType = 0x2B18, // Size: 0x1, Flags: 0x402
            BagSlotFlags = 0x2B1C, // Size: 0x4, Flags: 0x2
            BankBagSlotFlags = 0x2B2C, // Size: 0x7, Flags: 0x2
            InsertItemsLeftToRight = 0x2B48, // Size: 0x1, Flags: 0x2
            QuestCompleted = 0x2B4C, // Size: 0x36B, Flags: 0x2
            Honor = 0x38F8, // Size: 0x1, Flags: 0x2
            HonorNextLevel = 0x38FC // Size: 0x1, Flags: 0x2
        }

        internal enum SceneObjectData
        {
            ScriptPackageId = 0x30, // Size: 0x1, Flags: 0x1
            RndSeedVal = 0x34, // Size: 0x1, Flags: 0x1
            CreatedBy = 0x38, // Size: 0x4, Flags: 0x1
            SceneType = 0x48 // Size: 0x1, Flags: 0x1
        }

        internal enum Unit
        {
            CurrentCastId = 0x1B98,
            CurrentChanneledId = 0x1BB8,
            AuraTable = 0x1D10,
            AuraCount = 0x2390,
            AuraSize = 0x68,
            ClientRace = 0x2670,
            DisplayData = 0x1718
        }

        // Note: Invalid possibly!
        internal enum UnitAuras : uint
        {
            AuraCount1 = 0x2390,
            AuraCount2 = 0x1D10,
            AuraTable1 = 0x1D14,
            AuraTable2 = 0x1D18,
            AuraSize = 0x68,

            OwnerGuid = 0x40,
            AuraSpellId = 0x50,
            //AuraFlags = 0x54, //Not exactly sure here.
            //AuraLevel = 0x58, //Not exactly sure here.
            AuraStack = 0x59,
            TimeLeft = 0x60,
            //In case I need it:
            DruidEclipse = 0x2694
        }

        // Below is all of the World of Warcraft in-game object field offsets.
        // Commenting is not used on purpose and enums below should remain internal.
        internal enum UnitData
        {
            Charm = 0x30, // Size: 0x4, Flags: 0x1
            Summon = 0x40, // Size: 0x4, Flags: 0x1
            Critter = 0x50, // Size: 0x4, Flags: 0x2
            CharmedBy = 0x60, // Size: 0x4, Flags: 0x1
            SummonedBy = 0x70, // Size: 0x4, Flags: 0x1
            CreatedBy = 0x80, // Size: 0x4, Flags: 0x1
            DemonCreator = 0x90, // Size: 0x4, Flags: 0x1
            Target = 0xA0, // Size: 0x4, Flags: 0x1
            BattlePetCompanionGuid = 0xB0, // Size: 0x4, Flags: 0x1
            BattlePetDbid = 0xC0, // Size: 0x2, Flags: 0x1
            ChannelObject = 0xC8, // Size: 0x4, Flags: 0x201
            ChannelSpell = 0xD8, // Size: 0x1, Flags: 0x201
            ChannelSpellXSpellVisual = 0xDC, // Size: 0x1, Flags: 0x201
            SummonedByHomeRealm = 0xE0, // Size: 0x1, Flags: 0x1
            Sex = 0xE4, // Size: 0x1, Flags: 0x1
            DisplayPower = 0xE8, // Size: 0x1, Flags: 0x1
            OverrideDisplayPowerId = 0xEC, // Size: 0x1, Flags: 0x1
            Health = 0xF0, // Size: 0x2, Flags: 0x1
            Power = 0xF8, // Size: 0x6, Flags: 0x401
            TertiaryPower = 0xFC,
            SecondaryPower = 0x100,
            MaxHealth = 0x110, // Size: 0x2, Flags: 0x1
            MaxPower = 0x118, // Size: 0x6, Flags: 0x1
            PowerRegenFlatModifier = 0x130, // Size: 0x6, Flags: 0x46
            PowerRegenInterruptedFlatModifier = 0x148, // Size: 0x6, Flags: 0x46
            Level = 0x160, // Size: 0x1, Flags: 0x1
            EffectiveLevel = 0x164, // Size: 0x1, Flags: 0x1
            ScalingLevelMin = 0x168, // Size: 0x1, Flags: 0x1
            ScalingLevelMax = 0x16C, // Size: 0x1, Flags: 0x1
            ScalingLevelDelta = 0x170, // Size: 0x1, Flags: 0x1
            FactionTemplate = 0x174, // Size: 0x1, Flags: 0x1
            VirtualItems = 0x178, // Size: 0x6, Flags: 0x1
            Flags = 0x190, // Size: 0x1, Flags: 0x201
            Flags2 = 0x194, // Size: 0x1, Flags: 0x201
            Flags3 = 0x198, // Size: 0x1, Flags: 0x201
            AuraState = 0x19C, // Size: 0x1, Flags: 0x1
            AttackRoundBaseTime = 0x1A0, // Size: 0x2, Flags: 0x1
            RangedAttackRoundBaseTime = 0x1A8, // Size: 0x1, Flags: 0x2
            BoundingRadius = 0x1AC, // Size: 0x1, Flags: 0x1
            CombatReach = 0x1B0, // Size: 0x1, Flags: 0x1
            DisplayId = 0x1B4, // Size: 0x1, Flags: 0x280
            NativeDisplayId = 0x1B8, // Size: 0x1, Flags: 0x201
            MountDisplayId = 0x1BC, // Size: 0x1, Flags: 0x201
            MinDamage = 0x1C0, // Size: 0x1, Flags: 0x16
            MaxDamage = 0x1C4, // Size: 0x1, Flags: 0x16
            MinOffHandDamage = 0x1C8, // Size: 0x1, Flags: 0x16
            MaxOffHandDamage = 0x1CC, // Size: 0x1, Flags: 0x16
            AnimTier = 0x1D0, // Size: 0x1, Flags: 0x1
            PetNumber = 0x1D4, // Size: 0x1, Flags: 0x1
            PetNameTimestamp = 0x1D8, // Size: 0x1, Flags: 0x1
            PetExperience = 0x1DC, // Size: 0x1, Flags: 0x4
            PetNextLevelExperience = 0x1E0, // Size: 0x1, Flags: 0x4
            ModCastingSpeed = 0x1E4, // Size: 0x1, Flags: 0x1
            ModSpellHaste = 0x1E8, // Size: 0x1, Flags: 0x1
            ModHaste = 0x1EC, // Size: 0x1, Flags: 0x1
            ModRangedHaste = 0x1F0, // Size: 0x1, Flags: 0x1
            ModHasteRegen = 0x1F4, // Size: 0x1, Flags: 0x1
            ModTimeRate = 0x1F8, // Size: 0x1, Flags: 0x1
            CreatedBySpell = 0x1FC, // Size: 0x1, Flags: 0x1
            NpcFlags = 0x200, // Size: 0x2, Flags: 0x81
            EmoteState = 0x208, // Size: 0x1, Flags: 0x1
            Stats = 0x20C, // Size: 0x4, Flags: 0x6
            StatPosBuff = 0x21C, // Size: 0x4, Flags: 0x6
            StatNegBuff = 0x22C, // Size: 0x4, Flags: 0x6
            Resistances = 0x23C, // Size: 0x7, Flags: 0x16
            ResistanceBuffModsPositive = 0x258, // Size: 0x7, Flags: 0x6
            ResistanceBuffModsNegative = 0x274, // Size: 0x7, Flags: 0x6
            ModBonusArmor = 0x290, // Size: 0x1, Flags: 0x6
            BaseMana = 0x294, // Size: 0x1, Flags: 0x1
            BaseHealth = 0x298, // Size: 0x1, Flags: 0x6
            ShapeshiftForm = 0x29C, // Size: 0x1, Flags: 0x1
            AttackPower = 0x2A0, // Size: 0x1, Flags: 0x6
            AttackPowerModPos = 0x2A4, // Size: 0x1, Flags: 0x6
            AttackPowerModNeg = 0x2A8, // Size: 0x1, Flags: 0x6
            AttackPowerMultiplier = 0x2AC, // Size: 0x1, Flags: 0x6
            RangedAttackPower = 0x2B0, // Size: 0x1, Flags: 0x6
            RangedAttackPowerModPos = 0x2B4, // Size: 0x1, Flags: 0x6
            RangedAttackPowerModNeg = 0x2B8, // Size: 0x1, Flags: 0x6
            RangedAttackPowerMultiplier = 0x2BC, // Size: 0x1, Flags: 0x6
            SetAttackSpeedAura = 0x2C0, // Size: 0x1, Flags: 0x6
            MinRangedDamage = 0x2C4, // Size: 0x1, Flags: 0x6
            MaxRangedDamage = 0x2C8, // Size: 0x1, Flags: 0x6
            PowerCostModifier = 0x2CC, // Size: 0x7, Flags: 0x6
            PowerCostMultiplier = 0x2E8, // Size: 0x7, Flags: 0x6
            MaxHealthModifier = 0x304, // Size: 0x1, Flags: 0x6
            HoverHeight = 0x308, // Size: 0x1, Flags: 0x1
            MinItemLevelCutoff = 0x30C, // Size: 0x1, Flags: 0x1
            MinItemLevel = 0x310, // Size: 0x1, Flags: 0x1
            MaxItemLevel = 0x314, // Size: 0x1, Flags: 0x1
            WildBattlePetLevel = 0x318, // Size: 0x1, Flags: 0x1
            BattlePetCompanionNameTimestamp = 0x31C, // Size: 0x1, Flags: 0x1
            InteractSpellId = 0x320, // Size: 0x1, Flags: 0x1
            StateSpellVisualId = 0x324, // Size: 0x1, Flags: 0x280
            StateAnimId = 0x328, // Size: 0x1, Flags: 0x280
            StateAnimKitId = 0x32C, // Size: 0x1, Flags: 0x280
            StateWorldEffectId = 0x330, // Size: 0x4, Flags: 0x280
            ScaleDuration = 0x340, // Size: 0x1, Flags: 0x1
            LooksLikeMountId = 0x344, // Size: 0x1, Flags: 0x1
            LooksLikeCreatureId = 0x348, // Size: 0x1, Flags: 0x1
            LookAtControllerId = 0x34C, // Size: 0x1, Flags: 0x1
            LookAtControllerTarget = 0x350 // Size: 0x4, Flags: 0x1
        }
    }
}