using System.Windows;
using Artemis.VisualScripting.Attributes;
using Artemis.VisualScripting.Model;

namespace Artemis.VisualScripting.Nodes
{
    [UI("Integer-Value", "Outputs an configurable integer value.")]
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
            RegisterCustomView(Application.Current.FindResource("StaticValueCustomViewTemplate") as DataTemplate, _customViewModel);
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            Output.Value = _customViewModel.Input;
        }

        #endregion
    }

    [UI("Double-Value", "Outputs a configurable double value.")]
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
            RegisterCustomView(Application.Current.FindResource("StaticValueCustomViewTemplate") as DataTemplate, _customViewModel);
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            Output.Value = _customViewModel.Input;
        }

        #endregion
    }

    [UI("String-Value", "Outputs a configurable string value.")]
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
            RegisterCustomView(Application.Current.FindResource("StaticValueCustomViewTemplate") as DataTemplate, _customViewModel);
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

    public class StaticIntegerValueNodeCustomViewModel
    {
        public int Input { get; set; }
    }

    public class StaticDoubleValueNodeCustomViewModel
    {
        public double Input { get; set; }
    }

    public class StaticStringValueNodeCustomViewModel
    {
        public string Input { get; set; }
    }

    #endregion
}
