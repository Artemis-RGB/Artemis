using System.Text.RegularExpressions;
using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Text;

[Node("Regex Match", "Checks provided regex pattern matches the input.", "Text", InputType = typeof(string), OutputType = typeof(bool))]
public class StringRegexMatchNode : Node
{
    private string? _lastPattern;
    private Regex? _regex;
    private bool _broken;

    public StringRegexMatchNode() : base("Regex Match", "Checks provided regex pattern matches the input.")
    {
        Pattern = CreateInputPin<string>("Pattern");
        Input = CreateInputPin<string>("Input");
        Result = CreateOutputPin<bool>();
    }

    public InputPin<string> Pattern { get; }
    public InputPin<string> Input { get; }
    public OutputPin<bool> Result { get; }

    public override void Evaluate()
    {
        if (Input.Value == null || Pattern.Value == null)
            return;
        if (_broken && _lastPattern == Pattern.Value)
            return;

        if (_regex == null || _lastPattern != Pattern.Value)
        {
            try
            {
                _regex = new Regex(Pattern.Value, RegexOptions.Compiled);
                _broken = false;
            }
            catch (Exception)
            {
                _broken = true;
                return;
            }
            finally
            {
                _lastPattern = Pattern.Value;
            }
        }

        Result.Value = _regex.IsMatch(Input.Value);
    }
}