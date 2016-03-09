using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Models;

namespace Artemis.Modules.Games.Dota2
{
    class Dota2Settings : GameSettings
    {

        public Dota2Settings()
        {
            Load();
        }


        #region Variables
        public string GameDirectory { get; set; }
        public bool CanCastAbility { get; set; }
        public bool ShowHealth { get; set; }
        #endregion


        public override void Load()
        {
            ShowHealth = Dota2.Default.ShowHealth;
            CanCastAbility = Dota2.Default.CanCastAbility;
            Enabled = Dota2.Default.Enabled;
            GameDirectory = Dota2.Default.GameDirectory;
        }

        public override void Save()
        {
            Dota2.Default.ShowHealth = ShowHealth;
            Dota2.Default.CanCastAbility = CanCastAbility;
            Dota2.Default.Enabled = Enabled;
            Dota2.Default.GameDirectory = GameDirectory;
        }

        public override void ToDefault()
        {
            ShowHealth = true;
            CanCastAbility = true;
            Enabled = false;
            GameDirectory = string.Empty;
        }
    }
}
