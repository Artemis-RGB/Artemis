using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Artemis.ItemBehaviours
{
    /// <summary>
    ///     Chaitanya Kadamati - http://stackoverflow.com/a/33233162/5015269
    /// </summary>
    public class BindableSelectedItemBehavior : Behavior<TreeView>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
                AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItem = e.NewValue;
        }

        #region SelectedItem Property

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(BindableSelectedItemBehavior),
                new UIPropertyMetadata(null, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var behavior = sender as BindableSelectedItemBehavior;
            var tree = behavior?.AssociatedObject;
            if (tree == null)
                return;

            if (e.NewValue == null)
            {
                foreach (var item in tree.Items.OfType<TreeViewItem>())
                    item.SetValue(TreeViewItem.IsSelectedProperty, false);
            }
            var treeViewItem = e.NewValue as TreeViewItem;
            if (treeViewItem != null)
                treeViewItem.SetValue(TreeViewItem.IsSelectedProperty, true);
            else
            {
                var itemsHostProperty = tree.GetType()
                    .GetProperty("ItemsHost", BindingFlags.NonPublic | BindingFlags.Instance);
                var itemsHost = itemsHostProperty?.GetValue(tree, null) as Panel;
                if (itemsHost == null)
                    return;

                foreach (var item in itemsHost.Children.OfType<TreeViewItem>())
                {
                    if (WalkTreeViewItem(item, e.NewValue))
                        break;
                }
            }
        }

        public static bool WalkTreeViewItem(TreeViewItem treeViewItem, object selectedValue)
        {
            if (treeViewItem.DataContext == selectedValue)
            {
                treeViewItem.SetValue(TreeViewItem.IsSelectedProperty, true);
                treeViewItem.Focus();
                return true;
            }
            var itemsHostProperty = treeViewItem.GetType()
                .GetProperty("ItemsHost", BindingFlags.NonPublic | BindingFlags.Instance);
            var itemsHost = itemsHostProperty?.GetValue(treeViewItem, null) as Panel;
            if (itemsHost == null) return false;
            foreach (var item in itemsHost.Children.OfType<TreeViewItem>())
            {
                if (WalkTreeViewItem(item, selectedValue))
                    break;
            }
            return false;
        }

        #endregion
    }
}