using Artemis.Core;
using Artemis.VisualScripting.Nodes.Mathematics.Screens;
using NoStringEvaluating.Contract;
using NoStringEvaluating.Models.FormulaChecker;

namespace Artemis.VisualScripting.Nodes.Mathematics;

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