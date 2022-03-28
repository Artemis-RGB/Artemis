using Artemis.Core;
using Artemis.VisualScripting.Nodes.Maths.CustomViewModels;
using NoStringEvaluating.Contract;
using NoStringEvaluating.Contract.Variables;
using NoStringEvaluating.Models.FormulaChecker;
using NoStringEvaluating.Models.Values;

namespace Artemis.VisualScripting.Nodes.Maths;

[Node("Math Expression", "Outputs the result of a math expression.", "Mathematics", InputType = typeof(Numeric), OutputType = typeof(Numeric))]
public class MathExpressionNode : Node<string, MathExpressionNodeCustomViewModel>
{
    private readonly INoStringEvaluator _evaluator;
    private readonly IFormulaChecker _checker;
    private readonly PinsVariablesContainer _variables;

    #region Constructors

    public MathExpressionNode(INoStringEvaluator evaluator, IFormulaChecker checker)
        : base("Math Expression", "Outputs the result of a math expression.")
    {
        _evaluator = evaluator;
        _checker = checker;
        Output = CreateOutputPin<Numeric>();
        Values = CreateInputPinCollection<Numeric>("Values", 2);
        Values.PinAdded += (_, _) => SetPinNames();
        Values.PinRemoved += (_, _) => SetPinNames();
        _variables = new PinsVariablesContainer(Values);

        SetPinNames();
    }

    #endregion

    #region Properties & Fields

    public OutputPin<Numeric> Output { get; }
    public InputPinCollection<Numeric> Values { get; }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        try
        {
            if (Storage != null)
                Output.Value = new Numeric(_evaluator.CalcNumber(Storage, _variables));
        }
        catch
        {
            Output.Value = new Numeric(0);
        }
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

    public bool IsSyntaxValid(string? s)
    {
        if (s == null)
            return true;

        if (!_checker.CheckSyntax(s).Ok)
            return false;

        try
        {
            _evaluator.CalcNumber(s, _variables);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GetSyntaxErrors(string? s)
    {
        if (s == null)
            return "";

        CheckFormulaResult? syntaxCheck = _checker.CheckSyntax(s);
        if (!syntaxCheck.Ok)
            return string.Join(",", syntaxCheck.Mistakes);

        try
        {
            _evaluator.CalcNumber(s, _variables);
            return "";
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}

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