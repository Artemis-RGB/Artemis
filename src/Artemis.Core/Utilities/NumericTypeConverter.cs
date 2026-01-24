using System;
using System.ComponentModel;
using System.Globalization;

namespace Artemis.Core;

/// <summary>
///     Type converter for <see cref="Numeric" /> to support Avalonia binding from other numeric types
/// </summary>
public class NumericTypeConverter : TypeConverter
{
    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(double) ||
               sourceType == typeof(float) ||
               sourceType == typeof(int) ||
               sourceType == typeof(long) ||
               sourceType == typeof(byte) ||
               sourceType == typeof(short) ||
               sourceType == typeof(decimal) ||
               sourceType == typeof(uint) ||
               sourceType == typeof(ulong) ||
               sourceType == typeof(ushort) ||
               sourceType == typeof(sbyte) ||
               sourceType == typeof(string) ||
               base.CanConvertFrom(context, sourceType);
    }

    /// <inheritdoc />
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(double) ||
               destinationType == typeof(float) ||
               destinationType == typeof(int) ||
               destinationType == typeof(long) ||
               destinationType == typeof(byte) ||
               destinationType == typeof(short) ||
               destinationType == typeof(decimal) ||
               destinationType == typeof(uint) ||
               destinationType == typeof(ulong) ||
               destinationType == typeof(ushort) ||
               destinationType == typeof(sbyte) ||
               destinationType == typeof(string) ||
               base.CanConvertTo(context, destinationType);
    }

    /// <inheritdoc />
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        return value switch
        {
            double d => new Numeric(d),
            float f => new Numeric(f),
            int i => new Numeric(i),
            long l => new Numeric(l),
            byte b => new Numeric(b),
            short s => new Numeric(s),
            decimal dec => new Numeric((double)dec),
            uint ui => new Numeric(ui),
            ulong ul => new Numeric((long)ul),
            ushort us => new Numeric(us),
            sbyte sb => new Numeric(sb),
            string str => Numeric.TryParse(str, out Numeric result) ? result : throw new FormatException($"Unable to convert '{str}' to Numeric"),
            _ => base.ConvertFrom(context, culture, value)
        };
    }

    /// <inheritdoc />
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (value is Numeric numeric)
        {
            if (destinationType == typeof(double)) return (double)numeric;
            if (destinationType == typeof(float)) return (float)numeric;
            if (destinationType == typeof(int)) return (int)numeric;
            if (destinationType == typeof(long)) return (long)numeric;
            if (destinationType == typeof(byte)) return (byte)numeric;
            if (destinationType == typeof(short)) return (short)numeric;
            if (destinationType == typeof(decimal)) return (decimal)numeric;
            if (destinationType == typeof(uint)) return (uint)numeric;
            if (destinationType == typeof(ulong)) return (ulong)numeric;
            if (destinationType == typeof(ushort)) return (ushort)numeric;
            if (destinationType == typeof(sbyte)) return (sbyte)numeric;
            if (destinationType == typeof(string)) return numeric.ToString(CultureInfo.InvariantCulture);
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
}