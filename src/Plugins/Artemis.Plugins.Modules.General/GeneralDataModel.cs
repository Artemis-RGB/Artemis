using System.Collections.Generic;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using SkiaSharp;

namespace Artemis.Plugins.Modules.General
{
    public class GeneralDataModel : DataModel
    {
        public GeneralDataModel()
        {
            PlayerInfo = new PlayerInfo();
            IntsList = new List<int>();
            PlayerInfosList = new List<PlayerInfo>();
        }

        [DataModelProperty(Name = "A test string", Description = "This is a test string that's not of any use outside testing!")]
        public string TestString { get; set; }

        [DataModelProperty(Name = "A test boolean", Description = "This is a test boolean that's not of any use outside testing!")]
        public bool TestBoolean { get; set; }

        public SKColor TestColor { get; set; } = new SKColor(221, 21, 152);

        [DataModelProperty(Name = "Player info", Description = "[TEST] Contains information about the player")]
        public PlayerInfo PlayerInfo { get; set; }

        public double UpdatesDividedByFour { get; set; }
        public int Updates { get; set; }

        public List<int> IntsList { get; set; }
        public List<PlayerInfo> PlayerInfosList { get; set; }
    }

    public class PlayerInfo
    {
        [DataModelProperty(Name = "A test string", Description = "This is a test string that's not of any use outside testing!")]
        public string TestString { get; set; }

        [DataModelProperty(Name = "A test boolean", Description = "This is a test boolean that's not of any use outside testing!")]
        public bool TestBoolean { get; set; }

        [DataModelProperty(Affix = "%", MinValue = 0, MaxValue = 100)]
        public int Health { get; set; }

        public SKPoint Position { get; set; }
    }
}