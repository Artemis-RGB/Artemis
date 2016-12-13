using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Artemis.Dialogs
{
    /// <summary>
    ///     Interaction logic for MarkdownDialog.xaml
    /// </summary>
    public partial class MarkdownDialog : CustomDialog
    {
        public MetroWindow ParentWindow { get; set; }

        public static readonly DependencyProperty MarkdownProperty = DependencyProperty.Register("Markdown",
            typeof(string), typeof(MarkdownDialog), new PropertyMetadata(default(string)));

        public MarkdownDialog(MetroWindow parentWindow)
        {
            ParentWindow = parentWindow;
            InitializeComponent();

            Tcs = new TaskCompletionSource<MessageDialogResult>();

            CommandBindings.Add(new CommandBinding(NavigationCommands.GoToPage,
                (sender, e) => System.Diagnostics.Process.Start((string) e.Parameter)));
        }

        public TaskCompletionSource<MessageDialogResult> Tcs { get; set; }

        public string Markdown
        {
            get { return (string) GetValue(MarkdownProperty); }
            set { SetValue(MarkdownProperty, value); }
        }

        private void PART_AffirmativeButton_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.HideMetroDialogAsync(this);
        }
    }
}