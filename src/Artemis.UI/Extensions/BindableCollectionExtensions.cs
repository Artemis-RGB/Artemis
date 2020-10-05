using System;
using System.Collections.Generic;
using System.Linq;
using Stylet;

namespace Artemis.UI.Extensions
{
    public static class BindableCollectionExtensions
    {
        public static void Sort<T>(this BindableCollection<T> collection, Func<T, object> order)
        {
            List<T> ordered = collection.OrderBy(order).ToList();
            for (int index = 0; index < ordered.Count; index++)
            {
                T dataBindingConditionViewModel = ordered[index];
                if (collection.IndexOf(dataBindingConditionViewModel) != index)
                    collection.Move(collection.IndexOf(dataBindingConditionViewModel), index);
            }
        }
    }
}