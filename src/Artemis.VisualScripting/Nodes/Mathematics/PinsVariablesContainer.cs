using Artemis.Core;
using NoStringEvaluating.Contract.Variables;
using NoStringEvaluating.Models.Values;

namespace Artemis.VisualScripting.Nodes.Mathematics;

public class PinsVariablesContainer : IVariablesContainer
{
    private readonly InputPinCollection<Numeric> _values;

    public PinsVariablesContainer(InputPinCollection<Numeric> values)
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
        if (pin?.PinValue is Numeric numeric)
            return new EvaluatorValue(numeric);
        return new EvaluatorValue(0);
    }

    /// <inheritdoc />
    public bool TryGetValue(string name, out EvaluatorValue value)
    {
        IPin pin = _values.FirstOrDefault(v => v.Name == name);
        if (pin?.PinValue is Numeric numeric)
        {
            value = new EvaluatorValue(numeric);
            return true;
        }

        value = new EvaluatorValue(0);
        return false;
    }

    #endregion
}