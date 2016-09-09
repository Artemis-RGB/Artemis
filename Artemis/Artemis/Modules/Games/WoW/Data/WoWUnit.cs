using System;
using Process.NET;
using static Artemis.Modules.Games.WoW.Data.WoWEnums;
using static Artemis.Modules.Games.WoW.Data.WoWOffsets;

namespace Artemis.Modules.Games.WoW.Data
{
    public class WoWUnit : WoWObject
    {
        public WoWUnit(IProcess process, IntPtr baseAddress, bool readPointer = false)
            : base(process, baseAddress, readPointer)
        {
        }

        public int Health => ReadField<int>(UnitData.Health);
        public int MaxHealth => ReadField<int>(UnitData.MaxHealth);
        public int Power => ReadField<int>(UnitData.Power);
        public int SecondaryPower => ReadField<int>(UnitData.SecondaryPower);
        public int TertiaryPower => ReadField<int>(UnitData.TertiaryPower);
        public int MaxPower => ReadField<int>(UnitData.MaxPower);
        public PowerType PowerType => (PowerType) ReadField<int>(UnitData.DisplayPower);
        public int Level => ReadField<int>(UnitData.Level);

        public WoWDetails Details { get; set; }

        public void UpdateDetails(WoWNameCache nameCache)
        {
            Details = GetNpcDetails() ?? nameCache.GetNameByGuid(Guid);
        }
    }
}