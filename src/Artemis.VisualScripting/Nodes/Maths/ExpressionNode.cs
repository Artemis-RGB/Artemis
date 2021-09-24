using System;
using System.Linq;
using Artemis.Core;
using Artemis.VisualScripting.Nodes.Maths.CustomViewModels;
using NoStringEvaluating.Contract;
using NoStringEvaluating.Contract.Variables;
using NoStringEvaluating.Models.Values;

namespace Artemis.VisualScripting.Nodes.Maths
{
    [Node("Math Expression", "Outputs the result of a math expression.", "Math", OutputType = typeof(int))]
    public class MathExpressionNode : Node<string, MathExpressionNodeCustomViewModel>
    {
        private readonly INoStringEvaluator _evaluator;
        private readonly PinsVariablesContainer _variables;

        #region Constructors

        public MathExpressionNode(INoStringEvaluator evaluator)
            : base("Math Expression", "Outputs the result of a math expression.")
        {
            _evaluator = evaluator;
            Output = CreateOutputPin<float>();
            Values = CreateInputPinCollection<float>("Values", 2);
            Values.PinAdded += (_, _) => SetPinNames();
            Values.PinRemoved += (_, _) => SetPinNames();
            _variables = new PinsVariablesContainer(Values);

            SetPinNames();
        }

        #endregion

        #region Properties & Fields

        public OutputPin<float> Output { get; }
        public InputPinCollection<float> Values { get; }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            if (Storage != null)
                Output.Value = (float) _evaluator.CalcNumber(Storage, _variables);
        }

        private void SetPinNames()
        {
            int index = 1;
            foreach (IPin value in Values)
            {
                value.Name = ExcelColumnFromNumber(index).ToLower();
                index++;
            }
        }

        public static string ExcelColumnFromNumber(int column)
        {
            string columnString = "";
            decimal columnNumber = column;
            while (columnNumber > 0)
            {
                decimal currentLetterNumber = (columnNumber - 1) % 26;
                char currentLetter = (char) (currentLetterNumber + 65);
                columnString = currentLetter + columnString;
                columnNumber = (columnNumber - (currentLetterNumber + 1)) / 26;
            }

            return columnString;
        }

        #endregion
    }

    public class PinsVariablesContainer : IVariablesContainer
    {
        private readonly InputPinCollection<float> _values;

        public PinsVariablesContainer(InputPinCollection<float> values)
        {
            _values = values;
        }

        #region Implementation of IVariablesContainer

        /// <inheritdoc />
        public IVariable AddOrUpdate(string name, double value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public EvaluatorValue GetValue(string name)
        {
            IPin pin = _values.FirstOrDefault(v => v.Name == name);
            return pin == null ? new EvaluatorValue(0) : new EvaluatorValue((double) pin.PinValue);
        }

        /// <inheritdoc />
        public bool TryGetValue(string name, out EvaluatorValue value)
        {
            IPin pin = _values.FirstOrDefault(v => v.Name == name);
            double unboxed = (float) pin.PinValue;
            value = pin == null ? new EvaluatorValue(0) : new EvaluatorValue(unboxed);

            return pin != null;
        }

        #endregion
    }
}