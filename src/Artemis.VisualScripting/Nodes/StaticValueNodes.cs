using Artemis.Core;
using Artemis.VisualScripting.Nodes.CustomViewModels;

namespace Artemis.VisualScripting.Nodes
{
    [Node("Integer-Value", "Outputs an configurable integer value.")]
    public class StaticIntegerValueNode : Node<StaticIntegerValueNodeCustomViewModel>
    {
        #region Properties & Fields

        public OutputPin<int> Output { get; }

        #endregion

        #region Constructors

        public StaticIntegerValueNode()
            : base("Integer", "Outputs an configurable integer value.")
        {
            Output = CreateOutputPin<int>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            Output.Value = CustomViewModel.Input;
        }

        #endregion
    }

    [Node("Double-Value", "Outputs a configurable double value.")]
    public class StaticDoubleValueNode : Node<StaticDoubleValueNodeCustomViewModel>
    {
        #region Properties & Fields

        public OutputPin<double> Output { get; }

        #endregion

        #region Constructors

        public StaticDoubleValueNode()
            : base("Double", "Outputs a configurable double value.")
        {
            Output = CreateOutputPin<double>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            Output.Value = CustomViewModel.Input;
        }

        #endregion
    }

    [Node("String-Value", "Outputs a configurable string value.")]
    public class StaticStringValueNode : Node<StaticStringValueNodeCustomViewModel>
    {
        #region Properties & Fields

        public OutputPin<string> Output { get; }

        #endregion

        #region Constructors

        public StaticStringValueNode()
            : base("String", "Outputs a configurable string value.")
        {
            Output = CreateOutputPin<string>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            Output.Value = CustomViewModel.Input;
        }

        #endregion
    }

    #region CustomViewModels

    #endregion
}