using System;
using Artemis.Core;
using Material.Icons;
using SkiaSharp;

namespace Artemis.UI.Shared.Extensions;

/// <summary>
///     Provides utilities when working with types in UI elements.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    ///     Finds an appropriate Material Design icon for the given <paramref name="type" />.
    /// </summary>
    /// <param name="type">The type to retrieve an icon for.</param>
    /// <returns>An appropriate Material Design icon for the given <paramref name="type" />.</returns>
    public static MaterialIconKind GetTypeIcon(this Type? type)
    {
        if (type == null)
            return MaterialIconKind.QuestionMarkCircle;
        if (type.TypeIsNumber())
            return MaterialIconKind.CalculatorVariantOutline;
        if (type.IsEnum)
            return MaterialIconKind.FormatListBulletedSquare;
        if (type == typeof(bool))
            return MaterialIconKind.CircleHalfFull;
        if (type == typeof(string))
            return MaterialIconKind.Text;
        if (type == typeof(SKColor))
            return MaterialIconKind.Palette;
        if (type.IsAssignableTo(typeof(IDataModelEvent)))
            return MaterialIconKind.LightningBolt;
        return MaterialIconKind.Matrix;
    }
}