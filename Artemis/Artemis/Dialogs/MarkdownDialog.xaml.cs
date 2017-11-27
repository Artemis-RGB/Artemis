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
        public static readonly DependencyProperty MarkdownProperty = DependencyProperty.Register("Markdown", typeof(string), typeof(MarkdownDialog), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty AffirmativeButtonTextProperty = DependencyProperty.Register("AffirmativeButtonText", typeof(string), typeof(MarkdownDialog), new PropertyMetadata("OK"));
        public static readonly DependencyProperty NegativeButtonTextProperty = DependencyProperty.Register("NegativeButtonText", typeof(string), typeof(MarkdownDialog), new PropertyMetadata("Cancel"));

        public MarkdownDialog(MetroWindow parentWindow, MetroDialogSettings settings = null) : base(parentWindow, settings)
        {
            ParentWindow = parentWindow;
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(NavigationCommands.GoToPage, (sender, e) => System.Diagnostics.Process.Start((string) e.Parameter)));
            
            PART_AffirmativeButton.Click += (sender, args) => ParentWindow.HideMetroDialogAsync(this);
            PART_NegativeButton.Click += (sender, args) => ParentWindow.HideMetroDialogAsync(this);

            if (settings == null)
                return;
            AffirmativeButtonText = settings.AffirmativeButtonText;
            NegativeButtonText = settings.NegativeButtonText;
        }

        public MetroWindow ParentWindow { get; set; }

        public string Markdown
        {
            get => (string) GetValue(MarkdownProperty);
            set => SetValue(MarkdownProperty, value);
        }

        public string AffirmativeButtonText
        {
            get => (string) GetValue(AffirmativeButtonTextProperty);
            set => SetValue(AffirmativeButtonTextProperty, value);
        }

        public string NegativeButtonText
        {
            get => (string) GetValue(NegativeButtonTextProperty);
            set => SetValue(NegativeButtonTextProperty, value);
        }
    }
}
