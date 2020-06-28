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
        }

        [DataModelProperty(Name = "A test string", Description = "This is a test string that's not of any use outside testing!")]
        public string TestString { get; set; }

        [DataModelProperty(Name = "A test boolean", Description = "This is a test boolean that's not of any use outside testing!")]
        public bool TestBoolean { get; set; }

        [DataModelProperty(Name = "Player info", Description = "[TEST] Contains information about the player")]
        public PlayerInfo PlayerInfo { get; set; }

        public double UpdatesDividedByFour { get; set; }
    }

    public class PlayerInfo : DataModel
    {
        [DataModelProperty(Name = "A test string", Description = "This is a test string that's not of any use outside testing!")]
        public string TestString { get; set; }

        [DataModelProperty(Name = "A test boolean", Description = "This is a test boolean that's not of any use outside testing!")]
        public bool TestBoolean { get; set; }

        public SKPoint Position { get; set; }
    }
}