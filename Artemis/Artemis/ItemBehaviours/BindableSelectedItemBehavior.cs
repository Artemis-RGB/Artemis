using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Artemis.ItemBehaviours
{
    /// <summary>
    ///     Steve Greatrex - http://stackoverflow.com/a/5118406/5015269
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
            DependencyProperty.Register("SelectedItem", typeof (object), typeof (BindableSelectedItemBehavior),
                new UIPropertyMetadata(null, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var item = e.NewValue as TreeViewItem;
            item?.SetValue(TreeViewItem.IsSelectedProperty, true);
        }

        #endregion
    }
}