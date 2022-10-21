using System.ComponentModel;

namespace Artemis.VisualScripting.Nodes.Input;

public class PressedKeyPositionNodeEntity
{
    public PressedKeyPositionNodeEntity()
    {
    }

    public PressedKeyPositionNodeEntity(Guid layerId, KeyPressType respondTo)
    {
        LayerId = layerId;
        RespondTo = respondTo;
    }

    public Guid LayerId { get; set; }
    public KeyPressType RespondTo { get; set; }
    
    public enum KeyPressType
    {
        [Description("Up")]
        Up,
        [Description("Down")]
        Down,
        [Description("Up/down")]
        UpDown
    }
}