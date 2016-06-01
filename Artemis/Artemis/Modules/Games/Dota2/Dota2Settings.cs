using System.Windows.Media;
using Artemis.Models;

namespace Artemis.Modules.Games.Dota2
{
    internal class Dota2Settings : GameSettings
    {
        public Dota2Settings()
        {
            Load();
        }


        public override void Load()
        {
            KeyboardLayout = Dota2.Default.KeyboardLayout;
            MainColor = Dota2.Default.MainColor;
            ManaColor = Dota2.Default.ManaColor;
            ShowHealth = Dota2.Default.ShowHealth;
            CanCastAbility = Dota2.Default.CanCastAbility;
            Enabled = Dota2.Default.Enabled;
            GameDirectory = Dota2.Default.GameDirectory;
            ShowDayCycle = Dota2.Default.ShowDayCycle;
            ShowMana = Dota2.Default.ShowMana;
            ShowEvents = Dota2.Default.ShowEvents;
            AbilityReadyColor = Dota2.Default.AbilityReadyColor;
            AbilityCooldownColor = Dota2.Default.AbilityCooldownColor;
        }

        public override void Save()
        {
            Dota2.Default.Enabled = Enabled;
            Dota2.Default.LastProfile = LastProfile;
            Dota2.Default.GameDirectory = GameDirectory;

            Dota2.Default.KeyboardLayout = KeyboardLayout;
            Dota2.Default.MainColor = MainColor;
            Dota2.Default.ManaColor = ManaColor;
            Dota2.Default.ShowDayCycle = ShowDayCycle;
            Dota2.Default.ShowHealth = ShowHealth;
            Dota2.Default.CanCastAbility = CanCastAbility;
            Dota2.Default.ShowMana = ShowMana;
            Dota2.Default.ShowEvents = ShowEvents;
            Dota2.Default.AbilityCooldownColor = AbilityCooldownColor;
            Dota2.Default.AbilityReadyColor = AbilityReadyColor;

            Dota2.Default.Save();
        }

        public override void ToDefault()
        {
            Enabled = true;
            GameDirectory = string.Empty;

            KeyboardLayout = "Default";
            MainColor = Color.FromArgb(255, 255, 0, 0);
            ManaColor = Color.FromArgb(255, 0, 0, 255);
            AbilityCooldownColor = Color.FromArgb(255, 106, 90, 205);
            AbilityReadyColor = Color.FromArgb(255, 0, 255, 0);
            ShowHealth = true;
            CanCastAbility = true;
            ShowDayCycle = true;
            ShowMana = true;
            ShowEvents = true;
        }

        #region Variables

        public string GameDirectory { get; set; }
        public bool CanCastAbility { get; set; }
        public bool ShowHealth { get; set; }
        public bool ShowDayCycle { get; set; }
        public bool ShowMana { get; set; }
        public bool ShowEvents { get; set; }
        public Color MainColor { get; set; }
        public Color ManaColor { get; set; }
        public string KeyboardLayout { get; set; }
        public Color AbilityCooldownColor { get; set; }
        public Color AbilityReadyColor { get; set; }

        #endregion
    }
}