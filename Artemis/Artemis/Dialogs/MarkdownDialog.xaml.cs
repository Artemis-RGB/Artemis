using System.Windows;
using System.Windows.Input;

namespace Artemis.Dialogs
{
    /// <summary>
    ///     Interaction logic for MarkdownDialog.xaml
    /// </summary>
    public partial class MarkdownDialog
    {
        public static readonly DependencyProperty MarkdownProperty = DependencyProperty.Register("Markdown",
            typeof(string), typeof(MarkdownDialog), new PropertyMetadata(default(string)));

        public MarkdownDialog()
        {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(NavigationCommands.GoToPage,
                (sender, e) => System.Diagnostics.Process.Start((string) e.Parameter)));
        }

        public string Markdown
        {
            get { return (string) GetValue(MarkdownProperty); }
            set { SetValue(MarkdownProperty, value); }
        }
    }
}