using System;
using Artemis.Models.Interfaces;

namespace Artemis.Modules.Games.TheDivision
{
    public class TheDivisionDataModel : IGameDataModel
    {
        public TheDivisionDataModel()
        {
            TestyTest = new TestTest();
        }

        public PlayerState PartyMember1 { get; set; }
        public PlayerState PartyMember2 { get; set; }
        public PlayerState PartyMember3 { get; set; }

        public bool LowAmmo { get; set; }
        public bool LowHp { get; set; }
        public GrenadeState GrenadeState { get; set; }

        public TestTest TestyTest { get; set; }
    }

    
    public class TestTest
    {
        public string TestS { get; set; }
        public int TestI { get; set; }
    }

    public enum GrenadeState
    {
        HasGrenade,
        HasNoGrenade,
        GrenadeEquipped
    }

    public enum PlayerState
    {
        Offline,
        Online,
        Hit
    }
}