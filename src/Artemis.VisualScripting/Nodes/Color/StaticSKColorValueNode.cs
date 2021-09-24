using Artemis.Core;
using Artemis.VisualScripting.Nodes.Color.CustomViewModels;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color
{
    [Node("Color-Value", "Outputs a configurable color value.", "Static", InputType = typeof(SKColor), OutputType = typeof(SKColor))]
    public class StaticSKColorValueNode : Node<SKColor, StaticSKColorValueNodeCustomViewModel>
    {
        #region Constructors

        public StaticSKColorValueNode()
            : base("Color", "Outputs a configurable color value.")
        {
            Output = CreateOutputPin<SKColor>();
        }

        #endregion

        #region Properties & Fields

        public OutputPin<SKColor> Output { get; }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            Output.Value = Storage;
        }
        
        #endregion
    }
}