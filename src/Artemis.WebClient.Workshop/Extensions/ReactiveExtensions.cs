using System.Reactive.Linq;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.WebClient.Workshop.Extensions;

public static class ReactiveExtensions
{
    /// <summary>
    ///     Projects the data of the provided operation result into a new observable sequence if the result is successfull and
    ///     contains data.
    /// </summary>
    /// <param name="source">A sequence of operation results to invoke a transform function on.</param>
    /// <param name="selector">A transform function to apply to the data of each source element.</param>
    /// <typeparam name="TSource">The type of data contained in the operation result.</typeparam>
    /// <typeparam name="TResult">The type of data to project from the result.</typeparam>
    /// <returns>
    ///     An observable sequence whose elements are the result of invoking the transform function on each element of
    ///     source.
    /// </returns>
    public static IObservable<TResult> SelectOperationResult<TSource, TResult>(this IObservable<IOperationResult<TSource>> source, Func<TSource, TResult?> selector) where TSource : class
    {
        return source
            .Where(s => !s.Errors.Any())
            .Select(s => s.Data)
            .WhereNotNull()
            .Select(selector)
            .WhereNotNull();
    }
}