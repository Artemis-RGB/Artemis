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
            Output.Value = Storage as int? ?? 0;
        }

        public override void Initialize(INodeScript script) => Storage ??= 0;

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
            Output.Value = Storage as double? ?? 0.0;
        }

        public override void Initialize(INodeScript script) => Storage ??= 0.0;

        #endregion
    }

    [Node("Float-Value", "Outputs a configurable float value.")]
    public class StaticFloatValueNode : Node<StaticFloatValueNodeCustomViewModel>
    {
        #region Properties & Fields

        public OutputPin<float> Output { get; }

        #endregion

        #region Constructors

        public StaticFloatValueNode()
            : base("Float", "Outputs a configurable float value.")
        {
            Output = CreateOutputPin<float>();
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            if (Storage is double doubleValue)
                Storage = (float) doubleValue;

            Output.Value = Storage as float? ?? 0.0f;
        }

        public override void Initialize(INodeScript script) => Storage ??= 0.0f;

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
            Output.Value = Storage as string;
        }
        
        #endregion
    }

    #region CustomViewModels

    #endregion
}