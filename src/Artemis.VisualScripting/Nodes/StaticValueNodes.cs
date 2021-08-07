using Artemis.Core;
using Artemis.VisualScripting.Nodes.CustomViewModels;

namespace Artemis.VisualScripting.Nodes
{
    [Node("Integer-Value", "Outputs an configurable integer value.")]
    public class StaticIntegerValueNode : Node
    {
        #region Properties & Fields

        private StaticIntegerValueNodeCustomViewModel _customViewModel = new();

        public OutputPin<int> Output { get; }

        #endregion

        #region Constructors

        public StaticIntegerValueNode()
            : base("Integer", "Outputs an configurable integer value.")
        {
            Output = CreateOutputPin<int>();
            RegisterCustomViewModel<StaticIntegerValueNodeCustomViewModel>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            Output.Value = _customViewModel.Input;
        }

        #endregion
    }

    [Node("Double-Value", "Outputs a configurable double value.")]
    public class StaticDoubleValueNode : Node
    {
        #region Properties & Fields

        private StaticDoubleValueNodeCustomViewModel _customViewModel = new();

        public OutputPin<double> Output { get; }

        #endregion

        #region Constructors

        public StaticDoubleValueNode()
            : base("Double", "Outputs a configurable double value.")
        {
            Output = CreateOutputPin<double>();
            RegisterCustomViewModel<StaticDoubleValueNodeCustomViewModel>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            Output.Value = _customViewModel.Input;
        }

        #endregion
    }

    [Node("String-Value", "Outputs a configurable string value.")]
    public class StaticStringValueNode : Node
    {
        #region Properties & Fields

        private StaticStringValueNodeCustomViewModel _customViewModel = new();

        public OutputPin<string> Output { get; }

        #endregion

        #region Constructors

        public StaticStringValueNode()
            : base("String", "Outputs a configurable string value.")
        {
            Output = CreateOutputPin<string>();
            RegisterCustomViewModel<StaticStringValueNodeCustomViewModel>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            Output.Value = _customViewModel.Input;
        }

        #endregion
    }

    #region CustomViewModels



    #endregion
}
