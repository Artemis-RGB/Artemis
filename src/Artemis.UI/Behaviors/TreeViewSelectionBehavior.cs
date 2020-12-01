using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Artemis.UI.Behaviors
{
    public class TreeViewSelectionBehavior : Behavior<TreeView>
    {
        public delegate bool IsChildOfPredicate(object nodeA, object nodeB);

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object),
                typeof(TreeViewSelectionBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedItemChanged));

        public static readonly DependencyProperty HierarchyPredicateProperty =
            DependencyProperty.Register(nameof(HierarchyPredicate), typeof(IsChildOfPredicate),
                typeof(TreeViewSelectionBehavior),
                new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty ExpandSelectedProperty =
            DependencyProperty.Register(nameof(ExpandSelected), typeof(bool),
                typeof(TreeViewSelectionBehavior),
                new FrameworkPropertyMetadata(false));

        private readonly EventSetter _treeViewItemEventSetter;
        private bool _modelHandled;

        public TreeViewSelectionBehavior()
        {
            _treeViewItemEventSetter = new EventSetter(
                FrameworkElement.LoadedEvent,
                new RoutedEventHandler(OnTreeViewItemLoaded));
        }

        // Bindable selected item
        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        // Predicate that checks if two items are hierarchically related
        public IsChildOfPredicate HierarchyPredicate
        {
            get => (IsChildOfPredicate) GetValue(HierarchyPredicateProperty);
            set => SetValue(HierarchyPredicateProperty, value);
        }

        // Should expand selected?
        public bool ExpandSelected
        {
            get => (bool) GetValue(ExpandSelectedProperty);
            set => SetValue(ExpandSelectedProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
            ((INotifyCollectionChanged) AssociatedObject.Items).CollectionChanged += OnTreeViewItemsChanged;

            UpdateTreeViewItemStyle();
            _modelHandled = true;
            UpdateAllTreeViewItems();
            _modelHandled = false;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject != null)
            {
                AssociatedObject.ItemContainerStyle?.Setters?.Remove(_treeViewItemEventSetter);
                AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
                ((INotifyCollectionChanged) AssociatedObject.Items).CollectionChanged -= OnTreeViewItemsChanged;
            }
        }

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            TreeViewSelectionBehavior behavior = (TreeViewSelectionBehavior) sender;
            if (behavior._modelHandled) return;

            if (behavior.AssociatedObject == null)
                return;

            behavior._modelHandled = true;
            behavior.UpdateAllTreeViewItems();
            behavior._modelHandled = false;
        }

        // Update state of all items starting with given, with optional recursion
        private void UpdateTreeViewItem(TreeViewItem item, bool recurse)
        {
//            if (SelectedItem == null) return;
            object model = item.DataContext;

            // If the selected item is this model and is not yet selected - select and return
            if (SelectedItem == model && !item.IsSelected)
            {
                item.IsSelected = true;
                // Focus the newly selected item as if was clicked
                item.Focus();
                if (ExpandSelected)
                    item.IsExpanded = true;
            }
            else if (SelectedItem == model && !item.IsFocused)
            {
                // Focus the newly selected item as if was clicked
                item.Focus();
            }
            // If the selected item is a parent of this model - expand
            else
            {
                bool isParentOfModel = HierarchyPredicate?.Invoke(SelectedItem, model) ?? true;
                if (isParentOfModel)
                    item.IsExpanded = true;
            }

            // Recurse into children
            if (recurse)
                foreach (object subitem in item.Items)
                {
                    TreeViewItem tvi = item.ItemContainerGenerator.ContainerFromItem(subitem) as TreeViewItem;
                    if (tvi != null)
                        UpdateTreeViewItem(tvi, true);
                }
        }

        // Update state of all items
        private void UpdateAllTreeViewItems()
        {
            TreeView treeView = AssociatedObject;
            foreach (object item in treeView.Items)
            {
                TreeViewItem tvi = treeView.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (tvi != null)
                    UpdateTreeViewItem(tvi, true);
            }
        }

        // Inject Loaded event handler into ItemContainerStyle
        private void UpdateTreeViewItemStyle()
        {
            if (AssociatedObject.ItemContainerStyle == null)
                AssociatedObject.ItemContainerStyle = new Style(
                    typeof(TreeViewItem),
                    Application.Current.TryFindResource(typeof(TreeViewItem)) as Style);

            if (!AssociatedObject.ItemContainerStyle.Setters.Contains(_treeViewItemEventSetter))
                AssociatedObject.ItemContainerStyle.Setters.Add(_treeViewItemEventSetter);
        }

        private void OnTreeViewItemsChanged(object sender,
            NotifyCollectionChangedEventArgs args)
        {
            UpdateAllTreeViewItems();
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> args)
        {
            if (_modelHandled) return;
            if (AssociatedObject.Items.SourceCollection == null) return;
            SelectedItem = args.NewValue;
        }

        private void OnTreeViewItemLoaded(object sender, RoutedEventArgs args)
        {
            UpdateTreeViewItem((TreeViewItem) sender, false);
        }
    }
}