using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;

namespace Artemis.Modules.Games.Witcher3
{
    public class Witcher3Model : ModuleModel
    {
        private readonly Regex _configRegex;
        private readonly Stopwatch _updateSw;
        private string _witcherSettings;

        public Witcher3Model(DeviceManager deviceManager, LuaManager luaManager) : base(deviceManager, luaManager)
        {
            _updateSw = new Stopwatch();
            _configRegex = new Regex("\\[Artemis\\](.+?)\\[", RegexOptions.Singleline);

            Settings = SettingsProvider.Load<Witcher3Settings>();
            DataModel = new Witcher3DataModel();
            ProcessName = "witcher3";
        }

        public override string Name => "Witcher3";
        public override bool IsOverlay => false;
        public override bool IsBoundToProcess => true;

        public override void Dispose()
        {
            base.Dispose();

            _witcherSettings = null;
            _updateSw.Reset();
        }

        public override void Enable()
        {
            // Ensure the config file is found
            var witcherSettings = Environment.GetFolderPath(Environment.SpecialFolder.Personal) +
                                  @"\The Witcher 3\user.settings";
            if (File.Exists(witcherSettings))
                _witcherSettings = witcherSettings;

            _updateSw.Start();

            base.Enable();
        }

        public override void Update()
        {
            var gameDataModel = (Witcher3DataModel) DataModel;
            // Witcher effect is very static and reads from disk, don't want to update too often.
            if (_updateSw.ElapsedMilliseconds < 500)
                return;
            _updateSw.Restart();

            if (_witcherSettings == null)
                return;

            var reader =
                new StreamReader(File.Open(_witcherSettings, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            var configContent = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();

            var signRes = _configRegex.Match(configContent);
            var parts = signRes.Value.Split('\n').Skip(1).Select(v => v.Replace("\r", "")).ToList();
            parts.RemoveAt(parts.Count - 1);

            // Update sign
            var sign = parts.FirstOrDefault(p => p.Contains("ActiveSign="));
            if (sign != null)
            {
                var singString = sign.Split('=')[1];
                switch (singString)
                {
                    case "ST_Aard":
                        gameDataModel.WitcherSign = WitcherSign.Aard;
                        break;
                    case "ST_Yrden":
                        gameDataModel.WitcherSign = WitcherSign.Yrden;
                        break;
                    case "ST_Igni":
                        gameDataModel.WitcherSign = WitcherSign.Igni;
                        break;
                    case "ST_Quen":
                        gameDataModel.WitcherSign = WitcherSign.Quen;
                        break;
                    case "ST_Axii":
                        gameDataModel.WitcherSign = WitcherSign.Axii;
                        break;
                }
            }

            // Update max health
            var maxHealth = parts.FirstOrDefault(p => p.Contains("MaxHealth="));
            if (maxHealth != null)
            {
                var healthInt = int.Parse(maxHealth.Split('=')[1].Split('.')[0]);
                gameDataModel.MaxHealth = healthInt;
            }
            // Update health
            var health = parts.FirstOrDefault(p => p.Contains("Health="));
            if (health != null)
            {
                var healthInt = int.Parse(health.Split('=')[1].Split('.')[0]);
                gameDataModel.Health = healthInt;
            }
            // Update stamina
            var stamina = parts.FirstOrDefault(p => p.Contains("Stamina="));
            if (stamina != null)
            {
                var staminaInt = int.Parse(stamina.Split('=')[1].Split('.')[0]);
                gameDataModel.Stamina = staminaInt;
            }
            // Update stamina
            var toxicity = parts.FirstOrDefault(p => p.Contains("Toxicity="));
            if (toxicity != null)
            {
                var toxicityInt = int.Parse(toxicity.Split('=')[1].Split('.')[0]);
                gameDataModel.Toxicity = toxicityInt;
            }
            // Update vitality
            var vitality = parts.FirstOrDefault(p => p.Contains("Vitality="));
            if (vitality != null)
            {
                var vitalityInt = int.Parse(vitality.Split('=')[1].Split('.')[0]);
                gameDataModel.Vitality = vitalityInt;
            }
        }
    }
}