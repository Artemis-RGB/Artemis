using Artemis.Core;
using Artemis.VisualScripting.Nodes.Color.CustomViewModels;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color
{
    [Node("Color-Value", "Outputs a configurable color value.")]
    public class StaticSKColorValueNode : Node<StaticSKColorValueNodeCustomViewModel>
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
            Output.Value = Storage as SKColor? ?? SKColor.Empty;
        }

        public override void Initialize(INodeScript script)
        {
            if (Storage is string && SKColor.TryParse(Storage.ToString(), out SKColor parsed))
                Storage = parsed;
            else
                Storage = SKColor.Empty;
        }

        #endregion
    }
}