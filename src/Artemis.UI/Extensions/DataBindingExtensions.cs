using System;
using System.Reactive.Linq;
using Artemis.Core;

namespace Artemis.UI.Extensions;

public static class DataBindingExtensions
{
    public static IObservable<IDataBinding> AsObservable(this IDataBinding dataBinding)
    {
        return Observable.FromEventPattern<DataBindingEventArgs>(x => dataBinding.DataBindingEnabled += x, x => dataBinding.DataBindingEnabled -= x)
            .Merge(Observable.FromEventPattern<DataBindingEventArgs>(x => dataBinding.DataBindingDisabled += x, x => dataBinding.DataBindingDisabled -= x))
            .Select(e => e.EventArgs.DataBinding)
            .StartWith(dataBinding);
    }
}