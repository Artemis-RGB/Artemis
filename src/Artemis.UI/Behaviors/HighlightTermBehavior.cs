using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Artemis.UI.Behaviors
{
    // Source: https://stackoverflow.com/a/60474831/5015269
    // Made some changes to add a foreground and background property
    public static class HighlightTermBehavior
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(HighlightTermBehavior),
            new FrameworkPropertyMetadata("", OnTextChanged));

        public static readonly DependencyProperty TermToBeHighlightedProperty = DependencyProperty.RegisterAttached(
            "TermToBeHighlighted",
            typeof(string),
            typeof(HighlightTermBehavior),
            new FrameworkPropertyMetadata("", OnTextChanged));

        public static readonly DependencyProperty HighlightForegroundProperty = DependencyProperty.RegisterAttached(
            "HighlightForeground",
            typeof(Color?),
            typeof(HighlightTermBehavior),
            new FrameworkPropertyMetadata(null, OnTextChanged));

        public static readonly DependencyProperty HighlightBackgroundProperty = DependencyProperty.RegisterAttached(
            "HighlightBackground",
            typeof(Color?),
            typeof(HighlightTermBehavior),
            new FrameworkPropertyMetadata(null, OnTextChanged));

        public static string GetText(FrameworkElement frameworkElement)
        {
            return (string) frameworkElement.GetValue(TextProperty);
        }

        public static void SetText(FrameworkElement frameworkElement, string value)
        {
            frameworkElement.SetValue(TextProperty, value);
        }

        public static string GetTermToBeHighlighted(FrameworkElement frameworkElement)
        {
            return (string) frameworkElement.GetValue(TermToBeHighlightedProperty);
        }

        public static void SetTermToBeHighlighted(FrameworkElement frameworkElement, string value)
        {
            frameworkElement.SetValue(TermToBeHighlightedProperty, value);
        }

        public static void SetHighlightForeground(FrameworkElement frameworkElement, Color? value)
        {
            frameworkElement.SetValue(HighlightForegroundProperty, value);
        }

        public static Color? GetHighlightForeground(FrameworkElement frameworkElement)
        {
            return (Color?) frameworkElement.GetValue(HighlightForegroundProperty);
        }

        public static void SetHighlightBackground(FrameworkElement frameworkElement, Color? value)
        {
            frameworkElement.SetValue(HighlightBackgroundProperty, value);
        }

        public static Color? GetHighlightBackground(FrameworkElement frameworkElement)
        {
            return (Color?) frameworkElement.GetValue(HighlightBackgroundProperty);
        }

        public static List<string> SplitTextIntoTermAndNotTermParts(string text, string term)
        {
            if (string.IsNullOrEmpty(text))
                return new List<string> {string.Empty};

            return Regex.Split(text, $@"({Regex.Escape(term)})", RegexOptions.IgnoreCase)
                .Where(p => p != string.Empty)
                .ToList();
        }


        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
                SetTextBlockTextAndHighlightTerm(textBlock, GetText(textBlock), GetTermToBeHighlighted(textBlock));
        }

        private static void SetTextBlockTextAndHighlightTerm(TextBlock textBlock, string text, string termToBeHighlighted)
        {
            textBlock.Text = string.Empty;

            if (TextIsEmpty(text))
                return;

            if (TextIsNotContainingTermToBeHighlighted(text, termToBeHighlighted))
            {
                AddPartToTextBlock(textBlock, text);
                return;
            }

            List<string> textParts = SplitTextIntoTermAndNotTermParts(text, termToBeHighlighted);

            foreach (string textPart in textParts)
                AddPartToTextBlockAndHighlightIfNecessary(textBlock, termToBeHighlighted, textPart);
        }

        private static bool TextIsEmpty(string text)
        {
            return string.IsNullOrEmpty(text);
        }

        private static bool TextIsNotContainingTermToBeHighlighted(string text, string termToBeHighlighted)
        {
            if (text == null || termToBeHighlighted == null)
                return true;
            return text.Contains(termToBeHighlighted, StringComparison.OrdinalIgnoreCase) == false;
        }

        private static void AddPartToTextBlockAndHighlightIfNecessary(TextBlock textBlock, string termToBeHighlighted, string textPart)
        {
            if (textPart.Equals(termToBeHighlighted, StringComparison.OrdinalIgnoreCase))
                AddHighlightedPartToTextBlock(textBlock, textPart);
            else
                AddPartToTextBlock(textBlock, textPart);
        }

        private static void AddPartToTextBlock(TextBlock textBlock, string part)
        {
            textBlock.Inlines.Add(new Run {Text = part});
        }

        private static void AddHighlightedPartToTextBlock(TextBlock textBlock, string part)
        {
            Color? foreground = GetHighlightForeground(textBlock);
            Color? background = GetHighlightBackground(textBlock);

            if (background == null)
            {
                Run run = new() {Text = part, FontWeight = FontWeights.ExtraBold};
                if (foreground != null)
                    run.Foreground = new SolidColorBrush(foreground.Value);
                textBlock.Inlines.Add(run);
                return;
            }

            Border border = new()
            {
                Background = new SolidColorBrush(background.Value),
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(2),
                Child = new TextBlock {Text = part, FontWeight = FontWeights.Bold},
                Padding = new Thickness(1),
                Margin = new Thickness(-1, -5, -1, -5)
            };
            if (foreground != null)
                ((TextBlock) border.Child).Foreground = new SolidColorBrush(foreground.Value);
            textBlock.Inlines.Add(border);
        }
    }
}