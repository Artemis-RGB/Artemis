using System;
using System.Collections.Generic;
using System.Globalization;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a number, either decimal or not, with arbitrary precision.
    ///     <para>
    ///         Note: This struct is intended to be used by the node system when implementing your own <see cref="Node" />.
    ///         Usage outside that context is not recommended due to conversion overhead.
    ///     </para>
    /// </summary>
    public readonly struct Numeric : IComparable<Numeric>, IConvertible
    {
        private readonly float _value;

        #region Constructors

        /// <summary>
        ///     Creates a new instance of <see cref="Numeric" /> from a <see cref="float" />
        /// </summary>
        public Numeric(float value)
        {
            _value = value;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="Numeric" /> from an <see cref="int" />
        /// </summary>
        public Numeric(int value)
        {
            _value = value;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="Numeric" /> from a <see cref="double" />
        /// </summary>
        public Numeric(double value)
        {
            _value = (float)value;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="Numeric" /> from a <see cref="byte" />
        /// </summary>
        public Numeric(byte value)
        {
            _value = value;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="Numeric" /> from an <see cref="object" />
        /// </summary>
        public Numeric(object? pathValue)
        {
            _value = pathValue switch
            {
                float value => value,
                int value => value,
                double value => (float)value,
                byte value => value,
                Numeric value => value,
                _ => ParseFloatOrDefault(pathValue?.ToString())
            };
        }

        private static float ParseFloatOrDefault(string? pathValue)
        {
            float.TryParse(pathValue, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out float parsedFloat);
            return parsedFloat;
        }

        #endregion

        #region Relational members

        /// <inheritdoc />
        public int CompareTo(Numeric other)
        {
            return _value.CompareTo(other._value);
        }

        #endregion

        #region Equality members

        /// <summary>
        ///     Indicates whether this instance and a specified numeric are equal
        /// </summary>
        /// <param name="other">The numeric to compare with the current instance</param>
        /// <returns>
        ///     <see langword="true" /> if this numeric and the provided <paramref name="other" /> are equal; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public bool Equals(Numeric other)
        {
            return _value.Equals(other._value);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Numeric other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        #endregion

        #region Formatting members

        /// <inheritdoc />
        public override string ToString()
        {
            return _value.ToString(CultureInfo.InvariantCulture);
        }

        #endregion

        #region Operators

#pragma warning disable 1591

        public static implicit operator float(Numeric p)
        {
            return p._value;
        }

        public static implicit operator int(Numeric p)
        {
            return (int)MathF.Round(p._value, MidpointRounding.AwayFromZero);
        }

        public static implicit operator double(Numeric p)
        {
            return p._value;
        }

        public static implicit operator decimal(Numeric p)
        {
            return (decimal)p._value;
        }

        public static implicit operator byte(Numeric p)
        {
            return (byte)Math.Clamp(p._value, 0, 255);
        }

        public static implicit operator Numeric(double d) => new(d);
        public static implicit operator Numeric(float f) => new(f);
        public static implicit operator Numeric(int i) => new(i);
        public static implicit operator Numeric(byte b) => new(b);

        public static bool operator >(Numeric a, Numeric b)
        {
            return a._value > b._value;
        }

        public static bool operator <(Numeric a, Numeric b)
        {
            return a._value < b._value;
        }

        public static bool operator ==(Numeric left, Numeric right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Numeric left, Numeric right)
        {
            return !(left == right);
        }

        public static bool operator <=(Numeric left, Numeric right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(Numeric left, Numeric right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static Numeric operator +(Numeric a)
        {
            return new Numeric(+a._value);
        }

        public static Numeric operator -(Numeric a)
        {
            return new Numeric(-a._value);
        }

        public static Numeric operator ++(Numeric a)
        {
            return new Numeric(a._value + 1);
        }

        public static Numeric operator --(Numeric a)
        {
            return new Numeric(a._value - 1);
        }

        public static Numeric operator +(Numeric a, Numeric b)
        {
            return new Numeric(a._value + b._value);
        }

        public static Numeric operator -(Numeric a, Numeric b)
        {
            return new Numeric(a._value - b._value);
        }

        public static Numeric operator *(Numeric a, Numeric b)
        {
            return new Numeric(a._value * b._value);
        }

        public static Numeric operator %(Numeric a, Numeric b)
        {
            return new Numeric(a._value % b._value);
        }

        public static Numeric operator /(Numeric a, Numeric b)
        {
            if (b._value == 0)
                throw new DivideByZeroException();
            return new Numeric(a._value / b._value);
        }

#pragma warning restore 1591

        #endregion

        /// <summary>
        ///     Converts the string representation of a number into a numeric. A return value indicates whether the conversion
        ///     succeeded or failed.
        /// </summary>
        /// <param name="s">A string representing a number to convert.</param>
        /// <param name="result">
        ///     When this method returns, contains numeric equivalent to the numeric value or symbol contained in
        ///     <paramref name="s" />, if the conversion succeeded, or zero if the conversion failed.
        /// </param>
        /// <returns><see langword="true" /> if s was converted successfully; otherwise, <see langword="false" />.</returns>
        public static bool TryParse(string? s, out Numeric result)
        {
            bool parsed = float.TryParse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out float parsedFloat);
            if (!parsed)
            {
                result = new Numeric(0);
                return false;
            }

            result = new Numeric(parsedFloat);
            return true;
        }

        /// <summary>
        ///     Returns a boolean indicating whether the provided type can be used as a <see cref="Numeric" />.
        /// </summary>
        public static bool IsTypeCompatible(Type? type)
        {
            return type == typeof(Numeric) ||
                   type == typeof(float) ||
                   type == typeof(double) ||
                   type == typeof(int) ||
                   type == typeof(byte);
        }

        #region Implementation of IConvertible

        /// <inheritdoc />
        public TypeCode GetTypeCode()
        {
            return _value.GetTypeCode();
        }

        /// <inheritdoc />
        public bool ToBoolean(IFormatProvider? provider)
        {
            return Convert.ToBoolean(_value);
        }

        /// <inheritdoc />
        public byte ToByte(IFormatProvider? provider)
        {
            return (byte)Math.Clamp(_value, 0, 255);
        }

        /// <inheritdoc />
        public char ToChar(IFormatProvider? provider)
        {
            return Convert.ToChar(_value);
        }

        /// <inheritdoc />
        public DateTime ToDateTime(IFormatProvider? provider)
        {
            return Convert.ToDateTime(_value);
        }

        /// <inheritdoc />
        public decimal ToDecimal(IFormatProvider? provider)
        {
            return (decimal)_value;
        }

        /// <inheritdoc />
        public double ToDouble(IFormatProvider? provider)
        {
            return _value;
        }

        /// <inheritdoc />
        public short ToInt16(IFormatProvider? provider)
        {
            return (short)MathF.Round(_value, MidpointRounding.AwayFromZero);
        }

        /// <inheritdoc />
        public int ToInt32(IFormatProvider? provider)
        {
            return (int)MathF.Round(_value, MidpointRounding.AwayFromZero);
        }

        /// <inheritdoc />
        public long ToInt64(IFormatProvider? provider)
        {
            return (long)MathF.Round(_value, MidpointRounding.AwayFromZero);
        }

        /// <inheritdoc />
        public sbyte ToSByte(IFormatProvider? provider)
        {
            return (sbyte)Math.Clamp(_value, 0, 255);
        }

        /// <inheritdoc />
        public float ToSingle(IFormatProvider? provider)
        {
            return _value;
        }

        /// <inheritdoc />
        public string ToString(IFormatProvider? provider)
        {
            return _value.ToString(provider);
        }

        /// <inheritdoc />
        public object ToType(Type conversionType, IFormatProvider? provider)
        {
            return Convert.ChangeType(_value, conversionType);
        }

        /// <inheritdoc />
        public ushort ToUInt16(IFormatProvider? provider)
        {
            return (ushort)MathF.Round(_value, MidpointRounding.AwayFromZero);
        }

        /// <inheritdoc />
        public uint ToUInt32(IFormatProvider? provider)
        {
            return (uint)MathF.Round(_value, MidpointRounding.AwayFromZero);
        }

        /// <inheritdoc />
        public ulong ToUInt64(IFormatProvider? provider)
        {
            return (ulong)MathF.Round(_value, MidpointRounding.AwayFromZero);
        }

        #endregion
    }

    /// <summary>
    ///     Provides <see cref="Numeric" /> alternatives for common number-type extensions
    /// </summary>
    public static class NumericExtensions
    {
        #region Extensions

        /// <summary>
        ///     Sums the numerics in the provided collection
        /// </summary>
        /// <returns>The sum of all numerics in the collection</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Numeric Sum(this IEnumerable<Numeric> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            float sum = 0;
            foreach (float v in source)
                sum += v;

            return new Numeric(sum);
        }

        #endregion
    }
}