using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Artemis.UI.Extensions;

public static class ObservableCollectionExtensions
{
    public static void Sort<T>(this ObservableCollection<T> collection, Func<T, object> order)
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