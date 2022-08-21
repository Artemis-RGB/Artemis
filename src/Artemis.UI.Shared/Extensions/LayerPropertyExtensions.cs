using System;
using System.Reactive.Linq;
using Artemis.Core;

namespace Artemis.UI.Shared.Extensions;

/// <summary>
///     Provides utilities when working with layer properties in a UI context.
/// </summary>
public static class LayerPropertyExtensions
{
    /// <summary>
    ///     Returns an observable sequence of layer property values starting with the current value.
    /// </summary>
    /// <param name="layerProperty">The layer property to create the sequence of.</param>
    /// <typeparam name="T">The value type of the layer property.</typeparam>
    /// <returns>An observable sequence of layer property values starting with the current value.</returns>
    public static IObservable<T> AsObservable<T>(this LayerProperty<T> layerProperty)
    {
        return Observable.FromEventPattern<LayerPropertyEventArgs>(x => layerProperty.Updated += x, x => layerProperty.Updated -= x)
            .Select(_ => layerProperty.CurrentValue)
            .StartWith(layerProperty.CurrentValue);
    }
}