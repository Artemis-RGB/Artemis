using System.Globalization;

namespace Artemis.Core.Tests.VisualScripting;

public class NumericTests
{
    #region Creating

    [Fact]
    public void Numeric_CanBeCreatedFromEveryNumberType()
    {
        ((float) new Numeric(1.5f)).ShouldBe(1.5f);
        ((float) new Numeric(2)).ShouldBe(2f);
        ((float) new Numeric(2.5d)).ShouldBe(2.5f);
        ((float) new Numeric((byte) 200)).ShouldBe(200f);
        ((float) new Numeric(3L)).ShouldBe(3f);
    }

    [Theory]
    [InlineData(1.5f, 1.5f)]
    [InlineData(2, 2f)]
    [InlineData(2.5d, 2.5f)]
    [InlineData((byte) 7, 7f)]
    [InlineData(4L, 4f)]
    [InlineData("1.25", 1.25f)]
    [InlineData("1,000", 1000f)]
    [InlineData("not a number", 0f)]
    [InlineData(null, 0f)]
    [InlineData(true, 0f)]
    public void Numeric_FromAnObject_UsesNumbersAndParsesTheRest(object? value, float expected)
    {
        ((float) new Numeric(value)).ShouldBe(expected);
    }

    [Fact]
    public void Numeric_FromAnObject_CopiesANumeric()
    {
        ((float) new Numeric((object) new Numeric(3.5f))).ShouldBe(3.5f);
    }

    [Fact]
    public void Numeric_FromText_IgnoresTheCurrentCulture()
    {
        CultureInfo culture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("nl-NL");

            ((float) new Numeric("1.5")).ShouldBe(1.5f);
            new Numeric(1.5f).ToString().ShouldBe("1.5");
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    [Fact]
    public void TryParse_ReadsNumbers()
    {
        Numeric.TryParse("2.5", out Numeric parsed).ShouldBeTrue();
        parsed.ShouldBe(new Numeric(2.5f));

        Numeric.TryParse("abc", out Numeric failed).ShouldBeFalse();
        failed.ShouldBe(new Numeric(0));

        Numeric.TryParse(null, out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData(typeof(Numeric), true)]
    [InlineData(typeof(float), true)]
    [InlineData(typeof(double), true)]
    [InlineData(typeof(int), true)]
    [InlineData(typeof(long), true)]
    [InlineData(typeof(byte), true)]
    [InlineData(typeof(decimal), false)]
    [InlineData(typeof(short), false)]
    [InlineData(typeof(string), false)]
    [InlineData(null, false)]
    public void IsTypeCompatible_KnowsWhichTypesCanBeNumeric(Type? type, bool compatible)
    {
        Numeric.IsTypeCompatible(type).ShouldBe(compatible);
    }

    #endregion

    #region Converting

    [Theory]
    [InlineData(2.4f, 2)]
    [InlineData(2.5f, 3)]
    [InlineData(-2.5f, -3)]
    public void ToInt_RoundsHalfAwayFromZero(float value, int expected)
    {
        int converted = new Numeric(value);
        converted.ShouldBe(expected);
        AsConvertible(value).ToInt32(null).ShouldBe(expected);
    }

    [Fact]
    public void ToLong_CutsOffTheDecimals_UnlikeTheOtherWholeNumberConversions()
    {
        // Pinned as-is: turning a Numeric into a long cuts off the decimals, while turning it into an int rounds, and so
        // does the IConvertible conversion to a long
        long cast = new Numeric(2.7f);
        int castToInt = new Numeric(2.7f);

        cast.ShouldBe(2);
        castToInt.ShouldBe(3);
        AsConvertible(2.7f).ToInt64(null).ShouldBe(3);
    }

    [Theory]
    [InlineData(-5f, (byte) 0)]
    [InlineData(100.7f, (byte) 100)]
    [InlineData(300f, (byte) 255)]
    public void ToByte_KeepsTheValueBetween0And255(float value, byte expected)
    {
        byte converted = new Numeric(value);
        converted.ShouldBe(expected);
        AsConvertible(value).ToByte(null).ShouldBe(expected);
    }

    [Fact]
    public void ToSByte_KeepsTheValueBetween0And255_WhichDoesNotFitAnSByte()
    {
        // Pinned as-is: an sbyte holds -128 to 127, but the conversion clamps to 0-255 like a byte does. Anything above
        // 127 wraps around (200 becomes -56) and negative numbers become 0.
        AsConvertible(200f).ToSByte(null).ShouldBe((sbyte) -56);
        AsConvertible(-5f).ToSByte(null).ShouldBe((sbyte) 0);
        AsConvertible(100f).ToSByte(null).ShouldBe((sbyte) 100);
    }

    [Fact]
    public void Numeric_ConvertsToOtherNumberTypes()
    {
        Numeric numeric = new(2.5f);

        float asFloat = numeric;
        double asDouble = numeric;
        decimal asDecimal = numeric;
        asFloat.ShouldBe(2.5f);
        asDouble.ShouldBe(2.5d);
        asDecimal.ShouldBe(2.5m);
    }

    [Fact]
    public void Numeric_IsConvertible()
    {
        IConvertible numeric = AsConvertible(2.5f);

        numeric.GetTypeCode().ShouldBe(TypeCode.Single);
        numeric.ToSingle(null).ShouldBe(2.5f);
        numeric.ToDouble(null).ShouldBe(2.5d);
        numeric.ToDecimal(null).ShouldBe(2.5m);
        numeric.ToInt16(null).ShouldBe((short) 3);
        numeric.ToUInt16(null).ShouldBe((ushort) 3);
        numeric.ToUInt32(null).ShouldBe(3u);
        numeric.ToUInt64(null).ShouldBe(3ul);
        numeric.ToBoolean(null).ShouldBeTrue();
        AsConvertible(0).ToBoolean(null).ShouldBeFalse();
        numeric.ToString(CultureInfo.InvariantCulture).ShouldBe("2.5");
        numeric.ToType(typeof(int), null).ShouldBe(2);
        Convert.ChangeType(new Numeric(2.5f), typeof(double)).ShouldBe(2.5d);
    }

    [Fact]
    public void Numeric_CannotBecomeACharacterOrDate()
    {
        Should.Throw<InvalidCastException>(() => AsConvertible(65).ToChar(null));
        Should.Throw<InvalidCastException>(() => AsConvertible(1).ToDateTime(null));
    }

    [Fact]
    public void PassingANumericToConvert_PicksTheByteVersion()
    {
        // Pinned as-is: Numeric converts implicitly to byte, int, long, float, double and decimal, so the compiler sends
        // Convert.ToInt32(numeric) and friends to their byte versions. The value is clamped to 0-255 and loses its
        // decimals before it is converted. Passing it as an object uses IConvertible instead.
        Numeric numeric = new(2.5f);

        Convert.ToSingle(numeric).ShouldBe(2f);
        Convert.ToInt32(new Numeric(-10)).ShouldBe(0);
        Convert.ToDouble(new Numeric(1000)).ShouldBe(255d);
        Convert.ToSingle((object) numeric).ShouldBe(2.5f);
    }

    [Fact]
    public void NumberTypes_ConvertToNumeric()
    {
        Numeric fromDouble = 1.5d;
        Numeric fromFloat = 2.5f;
        Numeric fromInt = 3;
        Numeric fromByte = (byte) 4;

        fromDouble.ShouldBe(new Numeric(1.5f));
        fromFloat.ShouldBe(new Numeric(2.5f));
        fromInt.ShouldBe(new Numeric(3));
        fromByte.ShouldBe(new Numeric(4));
    }

    private static IConvertible AsConvertible(float value)
    {
        return new Numeric(value);
    }

    #endregion

    #region Math and comparing

    [Fact]
    public void Numeric_SupportsArithmetic()
    {
        Numeric a = new(7);
        Numeric b = new(2);

        (a + b).ShouldBe(new Numeric(9));
        (a - b).ShouldBe(new Numeric(5));
        (a * b).ShouldBe(new Numeric(14));
        (a / b).ShouldBe(new Numeric(3.5f));
        (a % b).ShouldBe(new Numeric(1));
        (-a).ShouldBe(new Numeric(-7));
        (+a).ShouldBe(new Numeric(7));
    }

    [Fact]
    public void Numeric_CanBeIncrementedAndDecremented()
    {
        Numeric value = new(1);

        value++;
        value.ShouldBe(new Numeric(2));
        value--;
        value--;
        value.ShouldBe(new Numeric(0));
    }

    [Fact]
    public void DividingByZero_Throws()
    {
        Should.Throw<DivideByZeroException>(() => new Numeric(1) / new Numeric(0));
    }

    [Fact]
    public void Numeric_CanBeCompared()
    {
        Numeric small = new(1);
        Numeric large = new(2);

        (small < large).ShouldBeTrue();
        (large > small).ShouldBeTrue();
        (small <= new Numeric(1)).ShouldBeTrue();
        (large >= new Numeric(2)).ShouldBeTrue();
        (small == new Numeric(1f)).ShouldBeTrue();
        (small != large).ShouldBeTrue();
        small.CompareTo(large).ShouldBeLessThan(0);
        small.Equals((object) new Numeric(1)).ShouldBeTrue();
        small.Equals((object) 1f).ShouldBeFalse();
        small.GetHashCode().ShouldBe(new Numeric(1).GetHashCode());
    }

    [Fact]
    public void Sum_AddsUpEveryNumeric()
    {
        new[] {new Numeric(1), new Numeric(2.5f), new Numeric(-1)}.Sum().ShouldBe(new Numeric(2.5f));
        Array.Empty<Numeric>().Sum().ShouldBe(new Numeric(0));
        Should.Throw<ArgumentNullException>(() => ((IEnumerable<Numeric>) null!).Sum());
    }

    [Fact]
    public void Subtract_TakesTheRestOffTheFirstNumeric()
    {
        new[] {new Numeric(10), new Numeric(3), new Numeric(2)}.Subtract().ShouldBe(new Numeric(5));
        new[] {new Numeric(4)}.Subtract().ShouldBe(new Numeric(4));
        Array.Empty<Numeric>().Subtract().ShouldBe(new Numeric(0));
        Should.Throw<ArgumentNullException>(() => ((IEnumerable<Numeric>) null!).Subtract());
    }

    #endregion
}
