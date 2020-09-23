using System;
using System.Linq;
using Stylet;

namespace Artemis.UI.Extensions
{
    public static class BindableCollectionExtensions
    {
        public static void Sort<T>(this BindableCollection<T> collection, Func<T, object> order)
        {
            var ordered = collection.OrderBy(order).ToList();
            for (var index = 0; index < ordered.Count; index++)
            {
                var dataBindingConditionViewModel = ordered[index];
                if (collection.IndexOf(dataBindingConditionViewModel) != index)
                    collection.Move(collection.IndexOf(dataBindingConditionViewModel), index);
            }
        }
    }
}