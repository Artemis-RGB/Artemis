using System.Collections;
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

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem",
            typeof (object), typeof (BindableSelectedItemBehavior), new UIPropertyMetadata(null, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var item = ((BindableSelectedItemBehavior) sender).AssociatedObject
                .ItemContainerGenerator.ContainerFromItem(e.NewValue) as TreeViewItem;
            if (item != null)
                item.SetValue(TreeViewItem.IsSelectedProperty, true);
            else
                ClearTreeViewSelection(((BindableSelectedItemBehavior) sender).AssociatedObject);
        }

        /// <summary>
        ///     Clears a TreeView's selected item recursively
        ///     Tom Wright - http://stackoverflow.com/a/1406116/5015269
        /// </summary>
        /// <param name="tv"></param>
        public static void ClearTreeViewSelection(TreeView tv)
        {
            if (tv != null)
                ClearTreeViewItemsControlSelection(tv.Items, tv.ItemContainerGenerator);
        }

        /// <summary>
        ///     Clears a TreeView's selected item recursively
        ///     Tom Wright - http://stackoverflow.com/a/1406116/5015269
        /// </summary>
        /// <param name="ic"></param>
        /// <param name="icg"></param>
        private static void ClearTreeViewItemsControlSelection(ICollection ic, ItemContainerGenerator icg)
        {
            if ((ic == null) || (icg == null))
                return;

            for (var i = 0; i < ic.Count; i++)
            {
                var tvi = icg.ContainerFromIndex(i) as TreeViewItem;
                if (tvi == null)
                    continue;
                ClearTreeViewItemsControlSelection(tvi.Items, tvi.ItemContainerGenerator);
                tvi.IsSelected = false;
            }
        }

        #endregion
    }
}