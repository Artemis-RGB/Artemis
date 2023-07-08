using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform;

namespace Artemis.UI.Shared.Routing;

/// <summary>
///     Represents a view model to which routing with parameters can take place and which in turn can host another view
///     model.
/// </summary>
/// <typeparam name="TScreen">The type of view model the screen can host.</typeparam>
/// <typeparam name="TParam">The type of parameters the screen expects. It must have a parameterless constructor.</typeparam>
public abstract class RoutableScreen<TScreen, TParam> : ActivatableViewModelBase, IRoutableScreen where TScreen : class where TParam : new()
{
    private bool _recycleScreen = true;
    private TScreen? _screen;

    /// <summary>
    ///     Gets the currently active child screen.
    /// </summary>
    public TScreen? Screen
    {
        get => _screen;
        private set => RaiseAndSetIfChanged(ref _screen, value);
    }

    /// <summary>
    ///     Called before navigating to this screen.
    /// </summary>
    /// <param name="args">Navigation arguments containing information about the navigation action.</param>
    public virtual Task BeforeNavigating(NavigationArguments args)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Called while navigating to this screen.
    /// </summary>
    /// <param name="parameters">An object containing the parameters of the navigation action.</param>
    /// <param name="args">Navigation arguments containing information about the navigation action.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used by other objects or threads to receive notice of
    ///     cancellation.
    /// </param>
    public virtual Task OnNavigating(TParam parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Called before navigating away from this screen.
    /// </summary>
    /// <param name="args">Navigation arguments containing information about the navigation action.</param>
    public virtual Task OnClosing(NavigationArguments args)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public bool RecycleScreen
    {
        get => _recycleScreen;
        protected set => RaiseAndSetIfChanged(ref _recycleScreen, value);
    }

    #region Overrides of RoutableScreen

    object? IRoutableScreen.InternalScreen => Screen;

    void IRoutableScreen.InternalChangeScreen(object? screen)
    {
        if (screen == null)
        {
            Screen = null;
            return;
        }

        if (screen is not TScreen typedScreen)
            throw new ArtemisRoutingException($"Provided screen is not assignable to {typeof(TScreen).FullName}");
        Screen = typedScreen;
    }

    async Task IRoutableScreen.InternalOnNavigating(NavigationArguments args, CancellationToken cancellationToken)
    {
        Func<object[], TParam> activator = GetParameterActivator();

        if (args.SegmentParameters.Length != _parameterPropertyCount)
            throw new ArtemisRoutingException($"Did not retrieve the required amount of parameters, expects {_parameterPropertyCount}, got {args.SegmentParameters.Length}.");

        TParam parameters = activator(args.SegmentParameters);
        await OnNavigating(parameters, args, cancellationToken);
    }

    async Task IRoutableScreen.InternalOnClosing(NavigationArguments args)
    {
        await OnClosing(args);
    }

    #endregion

    #region Parameter generation

    // ReSharper disable once StaticMemberInGenericType - That's intentional, each kind of TParam should have its own property count 
    private static int _parameterPropertyCount;
    private static Func<object[], TParam>? _parameterActivator;

    private static Func<object[], TParam> GetParameterActivator()
    {
        if (_parameterActivator != null)
            return _parameterActivator;

        // Generates a lambda that creates a new instance of TParam
        // - Each property of TParam with a public setter must be set using the source object[]
        // - Use the index of each property as the index on the source array
        // - Cast the object of the source array to the correct type for that property
        Type parameterType = typeof(TParam);
        ParameterExpression sourceExpression = Expression.Parameter(typeof(object[]), "source");
        ParameterExpression parameterExpression = Expression.Parameter(parameterType, "parameters");

        List<BinaryExpression> propertyAssignments = parameterType.GetProperties()
            .Where(p => p.CanWrite)
            .Select((property, index) =>
            {
                UnaryExpression sourceValueExpression = Expression.Convert(
                    Expression.ArrayIndex(sourceExpression, Expression.Constant(index)),
                    property.PropertyType
                );
                BinaryExpression propertyAssignment = Expression.Assign(
                    Expression.Property(parameterExpression, property),
                    sourceValueExpression
                );

                return propertyAssignment;
            })
            .ToList();

        Expression<Func<object[], TParam>> lambda = Expression.Lambda<Func<object[], TParam>>(
            Expression.Block(
                new[] {parameterExpression},
                Expression.Assign(parameterExpression, Expression.New(parameterType)),
                Expression.Block(propertyAssignments),
                parameterExpression
            ),
            sourceExpression
        );

        _parameterActivator = lambda.Compile();
        _parameterPropertyCount = propertyAssignments.Count;

        return _parameterActivator;
    }

    #endregion
}