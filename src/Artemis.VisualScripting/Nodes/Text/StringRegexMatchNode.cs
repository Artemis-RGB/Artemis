using System.Text.RegularExpressions;
using Artemis.Core;

namespace Artemis.VisualScripting.Nodes.Text;

[Node("Regex Match", "Checks provided regex pattern matches the input.", "Text", InputType = typeof(string), OutputType = typeof(bool))]
public class StringRegexMatchNode : Node
{
    private string? _lastPattern;
    private Regex? _regex;
    private Exception? _exception;

    public StringRegexMatchNode()
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

        // If the regex was invalid output false and rethrow the exception
        if (_lastPattern == Pattern.Value && _exception != null)
        {
            Result.Value = false;
            throw _exception;
        }

        // If there is no regex yet or the regex changed, recompile
        if (_regex == null || _lastPattern != Pattern.Value)
        {
            try
            {
                _regex = new Regex(Pattern.Value, RegexOptions.Compiled);
                _exception = null;
            }
            catch (Exception e)
            {
                // If there is an exception, save it to keep rethrowing until the regex is fixed
                _exception = e;
                throw;
            }
            finally
            {
                _lastPattern = Pattern.Value;
            }
        }

        Result.Value = _regex.IsMatch(Input.Value);
    }
}