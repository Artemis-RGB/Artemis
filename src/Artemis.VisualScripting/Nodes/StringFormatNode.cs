using System.Linq;
using Artemis.VisualScripting.Attributes;
using Artemis.VisualScripting.Model;

namespace Artemis.VisualScripting.Nodes
{
    [UI("Format", "Formats the input string.")]
    public class StringFormatNode : Node
    {
        #region Properties & Fields

        public InputPin<string> Format { get; }
        public InputPinCollection<object> Values { get; }

        public OutputPin<string> Output { get; }

        #endregion

        #region Constructors

        public StringFormatNode()
            : base("Format", "Formats the input string.")
        {
            Format = CreateInputPin<string>("Format");
            Values = CreateInputPinCollection<object>("Values", 1);
            Output = CreateOutputPin<string>("Result");
        }

        #endregion

        #region Methods

        public override void Evaluate()
        { 
            Output.Value = string.Format(Format.Value ?? string.Empty, Values.Values.ToArray());
        }

        #endregion
    }
}
