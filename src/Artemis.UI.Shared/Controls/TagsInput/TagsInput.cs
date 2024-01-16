using System.Text.RegularExpressions;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using ReactiveUI;

namespace Artemis.UI.Shared.TagsInput;

/// <summary>
/// Represents an input for tags.
/// </summary>
[TemplatePart("PART_TagInputBox", typeof(TextBox))]
public partial class TagsInput : TemplatedControl
{
    private TextBox? _tagInputBox;

    /// <summary>
    /// Gets the command that is to be called when removing a tag
    /// </summary>
    public ICommand RemoveTag { get; }

    /// <inheritdoc />
    public TagsInput()
    {
        RemoveTag = ReactiveCommand.Create<string>(ExecuteRemoveTag);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_tagInputBox != null)
        {
            _tagInputBox.KeyDown -= TagInputBoxOnKeyDown;
            _tagInputBox.TextChanging -= TagInputBoxOnTextChanging;
        }

        _tagInputBox = e.NameScope.Find<TextBox>("PART_TagInputBox");

        if (_tagInputBox != null)
        {
            _tagInputBox.KeyDown += TagInputBoxOnKeyDown;
            _tagInputBox.TextChanging += TagInputBoxOnTextChanging;
        }
    }

    private void ExecuteRemoveTag(string t)
    {
        Tags.Remove(t);

        if (_tagInputBox != null)
            _tagInputBox.IsEnabled = Tags.Count < MaxLength;
    }

    private void TagInputBoxOnTextChanging(object? sender, TextChangingEventArgs e)
    {
        if (_tagInputBox?.Text == null)
            return;

        _tagInputBox.Text = CleanTagRegex().Replace(_tagInputBox.Text.ToLower(), "");
    }

    private void TagInputBoxOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (_tagInputBox == null)
            return;

        if (e.Key == Key.Space)
            e.Handled = true;
        if (e.Key != Key.Enter)
            return;

        if (string.IsNullOrWhiteSpace(_tagInputBox.Text) || Tags.Contains(_tagInputBox.Text) || Tags.Count >= MaxLength)
            return;

        Tags.Add(CleanTagRegex().Replace(_tagInputBox.Text.ToLower(), ""));

        _tagInputBox.Text = "";
        _tagInputBox.IsEnabled = Tags.Count < MaxLength;
    }

    [GeneratedRegex("[\\s\\-]+")]
    private static partial Regex CleanTagRegex();
}