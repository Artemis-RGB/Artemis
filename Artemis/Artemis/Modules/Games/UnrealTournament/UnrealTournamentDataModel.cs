using System.ComponentModel;
using Artemis.Modules.Abstract;
using MoonSharp.Interpreter;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.UnrealTournament
{
    [MoonSharpUserData]
    public class UnrealTournamentDataModel : ModuleDataModel
    {
        public State State { get; set; }
        public Environment Environment { get; set; }
        public Player Player { get; set; }
    }

    public enum State
    {
        MainMenu,
        Spectating,
        Alive,
        Dead
    }

    [MoonSharpUserData]
    public class Player
    {
        public int Health { get; set; }
        public int Armor { get; set; }
        public PlayerState State { get; set; }
        public Inventory Inventory { get; set; }
        public Weapon Weapon { get; set; }

        [DefaultValue("None")]
        public KillState KillState { get; set; }
    }

    public enum KillState
    {
        None = 0,
        Kill = 1,
        DoubleKill = 2,
        MultiKill = 3,
        UltraKill = 4,
        MonsterKill = 5
    }

    [MoonSharpUserData]
    public class PlayerState
    {
        public string PlayerName { get; set; }
        public string UniqueId { get; set; }
        public int Score { get; set; }

        [JsonProperty("Team Num")]
        public int TeamNum { get; set; }

        public int RankCheck { get; set; }

        [JsonProperty("Duel Rank")]
        public int DuelRank { get; set; }

        public int No_of_Duel_Played { get; set; }

        [JsonProperty("CTF Rank")]
        public int CTFRank { get; set; }

        public int No_CTF_MatchesPlayed { get; set; }

        [JsonProperty("TDM Rank")]
        public int TDMRank { get; set; }

        public int No_TDM_MatchesPlayed { get; set; }
        public int DMRank { get; set; }
        public int No_DM_Matches_Played { get; set; }
        public int ShowdownRank { get; set; }
        public int No_Showdowns { get; set; }
    }

    [MoonSharpUserData]
    public class Inventory
    {
        public bool HasJumpBoots { get; set; }
        public bool HasInvisibility { get; set; }
        public bool HasBerserk { get; set; }
        public bool HasUDamage { get; set; }
        public bool HasThighPads { get; set; }
        public bool HasShieldBelt { get; set; }
        public bool HasChestArmor { get; set; }
        public bool HasHelmet { get; set; }
    }

    [MoonSharpUserData]
    public class Weapon
    {
        public string Name { get; set; }
        public int Ammo { get; set; }
        public int MaxAmmo { get; set; }
        public bool IsFiring { get; set; }
        public int FireMode { get; set; }
        public ZoomState ZoomState { get; set; }
    }

    public enum ZoomState
    {
        Unzoomed = 0,
        Zoomed = 3,
        ZoomingIn = 1,
        ZoomingOut = 2
    }

    [MoonSharpUserData]
    public class Environment
    {
        public string GameMode { get; set; }
        public bool MatchStarted { get; set; }
        public int GoalScore { get; set; }
        public string ServerName { get; set; }
        public bool bWeaponStay { get; set; }
        public bool bTeamGame { get; set; }
        public bool bAllowTeamSwitches { get; set; }
        public bool bStopGameClock { get; set; }
        public bool bCasterControl { get; set; }
        public bool bForcedBalance { get; set; }
        public bool bPlayPlayerIntro { get; set; }
        public float TimeLimit { get; set; }
        public float SpawnProtectionTime { get; set; }
        public float RemainingTime { get; set; }
        public float ElapsedTime { get; set; }
        public float RespawnWaitTime { get; set; }
        public float ForceRespawnTime { get; set; }
    }
}