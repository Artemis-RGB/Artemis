using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree
{
    public class ProfileTreeView : ReactiveUserControl<ProfileTreeViewModel>
    {
        private TreeView _treeView;

        public ProfileTreeView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            _treeView = this.Get<TreeView>("ProfileTreeView");
        }

        private void ProfileTreeView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            _treeView.Focus();
        }
    }
}