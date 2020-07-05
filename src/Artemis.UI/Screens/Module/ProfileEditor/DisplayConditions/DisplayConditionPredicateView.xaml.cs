﻿using System.Windows;
using System.Windows.Controls;

namespace Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions
{
    /// <summary>
    ///     Interaction logic for DisplayConditionPredicateView.xaml
    /// </summary>
    public partial class DisplayConditionPredicateView : UserControl
    {
        public DisplayConditionPredicateView()
        {
            InitializeComponent();
        }

        private void PropertyButton_OnClick(object sender, RoutedEventArgs e)
        {
            // DataContext is not set when using left button, I don't know why but there it is
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.DataContext = button.DataContext;
                button.ContextMenu.IsOpen = true;
            }
        }
    }
}