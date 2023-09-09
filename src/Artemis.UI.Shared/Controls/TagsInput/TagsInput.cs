using System.Text.RegularExpressions;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using ReactiveUI;

namespace Artemis.UI.Shared.TagsInput;

[TemplatePart("PART_TagInputBox", typeof(TextBox))]
public partial class TagsInput : TemplatedControl
{
    public TextBox? TagInputBox { get; set; }
    public ICommand RemoveTag { get; }

    /// <inheritdoc />
    public TagsInput()
    {
        RemoveTag = ReactiveCommand.Create<string>(ExecuteRemoveTag);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (TagInputBox != null)
        {
            TagInputBox.KeyDown -= TagInputBoxOnKeyDown;
            TagInputBox.TextChanging -= TagInputBoxOnTextChanging;
        }

        TagInputBox = e.NameScope.Find<TextBox>("PART_TagInputBox");

        if (TagInputBox != null)
        {
            TagInputBox.KeyDown += TagInputBoxOnKeyDown;
            TagInputBox.TextChanging += TagInputBoxOnTextChanging;
        }
    }

    private void ExecuteRemoveTag(string t)
    {
        Tags.Remove(t);

        if (TagInputBox != null)
            TagInputBox.IsEnabled = Tags.Count < MaxLength;
    }

    private void TagInputBoxOnTextChanging(object? sender, TextChangingEventArgs e)
    {
        if (TagInputBox?.Text == null)
            return;

        TagInputBox.Text = CleanTagRegex().Replace(TagInputBox.Text.ToLower(), "");
    }

    private void TagInputBoxOnKeyDown(object? sender, KeyEventArgs e)
    {
        if (TagInputBox == null)
            return;

        if (e.Key == Key.Space)
            e.Handled = true;
        if (e.Key != Key.Enter)
            return;

        if (string.IsNullOrWhiteSpace(TagInputBox.Text) || Tags.Contains(TagInputBox.Text) || Tags.Count >= MaxLength)
            return;

        Tags.Add(CleanTagRegex().Replace(TagInputBox.Text.ToLower(), ""));

        TagInputBox.Text = "";
        TagInputBox.IsEnabled = Tags.Count < MaxLength;
    }

    [GeneratedRegex("[\\s\\-]+")]
    private static partial Regex CleanTagRegex();
}