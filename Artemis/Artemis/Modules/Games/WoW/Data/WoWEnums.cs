namespace Artemis.Modules.Games.WoW.Data
{
    public static class WoWEnums
    {
        public enum GuidType : byte
        {
            Null = 0,
            Uniq = 1,
            Player = 2,
            Item = 3,
            StaticDoor = 4,
            Transport = 5,
            Conversation = 6,
            Creature = 7,
            Vehicle = 8,
            Pet = 9,
            GameObject = 10,
            DynamicObject = 11,
            AreaTrigger = 12,
            Corpse = 13,
            LootObject = 14,
            SceneObject = 15,
            Scenario = 16,
            AiGroup = 17,
            DynamicDoor = 18,
            ClientActor = 19,
            Vignette = 20,
            CallForHelp = 21,
            AiResource = 22,
            AiLock = 23,
            AiLockTicket = 24,
            ChatChannel = 25,
            Party = 26,
            Guild = 27,
            WowAccount = 28,
            BNetAccount = 29,
            GmTask = 30,
            MobileSession = 31,
            RaidGroup = 32,
            Spell = 33,
            Mail = 34,
            WebObj = 35,
            LfgObject = 36,
            LfgList = 37,
            UserRouter = 38,
            PvpQueueGroup = 39,
            UserClient = 40,
            PetBattle = 41,
            UniqueUserClient = 42,
            BattlePet = 43
        }

        public enum ObjectType
        {
            Object = 0,
            Item = 1,
            Container = 2,
            Unit = 3,
            Player = 4,
            GameObject = 5,
            DynamicObject = 6,
            Corpse = 7,
            AreaTrigger = 8,
            SceneObject = 9,
            Conversation = 10
        }

        public enum PowerType
        {
            Mana = 0,
            Rage = 1,
            Focus = 2,
            Energy = 3,
            Happiness = 4,
            RunicPower = 5,
            Runes = 6,
            Health = 7,
            Maelstrom = 11,
            Insanity = 13,
            Fury = 17,
            Pain = 18,
            UNKNOWN
        }

        public enum Reaction
        {
            Hostile = 1,
            Neutral = 3,
            Friendly = 4
        }

        public enum ShapeshiftForm
        {
            Normal = 0,
            Cat = 1,
            TreeOfLife = 2,
            Travel = 3,
            Aqua = 4,
            Bear = 5,
            Ambient = 6,
            Ghoul = 7,
            DireBear = 8,
            CreatureBear = 14,
            CreatureCat = 15,
            GhostWolf = 16,
            BattleStance = 17,
            DefensiveStance = 18,
            BerserkerStance = 19,
            EpicFlightForm = 27,
            Shadow = 28,
            Stealth = 30,
            Moonkin = 31,
            SpiritOfRedemption = 32
        }

        public enum WoWClass
        {
            None = 0,
            Warrior = 1,
            Paladin = 2,
            Hunter = 3,
            Rogue = 4,
            Priest = 5,
            DeathKnight = 6,
            Shaman = 7,
            Mage = 8,
            Warlock = 9,
            Druid = 11
        }

        public enum WoWRace
        {
            Human = 1,
            Orc = 2,
            Dwarf = 3,
            NightElf = 4,
            Undead = 5,
            Tauren = 6,
            Gnome = 7,
            Troll = 8,
            Goblin = 9,
            BloodElf = 10,
            Draenei = 11,
            FelOrc = 12,
            Naga = 13,
            Broken = 14,
            Skeleton = 15,
            Worgen = 22
        }

        public enum WoWType
        {
            Player,
            Npc
        }
    }
}