using System.Collections;
using SkiaSharp;

namespace Artemis.Core.Tests.Utilities;

public class TypeExtensionsTests
{
    private class GenericBase<T>;

    private class DerivedFromGeneric : GenericBase<int>;

    private class DerivedTwice : DerivedFromGeneric;

    private class Celsius
    {
        public static implicit operator Fahrenheit(Celsius c) => new();
    }

    private class Fahrenheit
    {
        public static explicit operator Fahrenheit(Kelvin k) => new();
    }

    private class Kelvin;

    #region Kinds of types

    [Fact]
    public void IsGenericType_ChecksTheBaseType()
    {
        typeof(DerivedFromGeneric).IsGenericType(typeof(GenericBase<>)).ShouldBeTrue();
        typeof(object).IsGenericType(typeof(GenericBase<>)).ShouldBeFalse();
        ((Type?) null).IsGenericType(typeof(GenericBase<>)).ShouldBeFalse();
    }

    [Fact]
    public void IsGenericType_ThrowsWhenTheBaseTypeIsNotGeneric()
    {
        // Pinned as-is: asking for the generic definition of a base type that isn't generic throws instead of returning false.
        // This includes types further down the line, only the direct base type is looked at
        Should.Throw<InvalidOperationException>(() => typeof(string).IsGenericType(typeof(GenericBase<>)));
        Should.Throw<InvalidOperationException>(() => typeof(DerivedTwice).IsGenericType(typeof(GenericBase<>)));
    }

    [Theory]
    [InlineData(typeof(SKColor), true)]
    [InlineData(typeof(DateTime), true)]
    [InlineData(typeof(int), false)]
    [InlineData(typeof(KeyboardSection), false)]
    [InlineData(typeof(string), false)]
    public void IsStruct_OnlyForStructsThatAreNotNumbersOrEnums(Type type, bool expected)
    {
        type.IsStruct().ShouldBe(expected);
    }

    [Theory]
    [InlineData(typeof(sbyte), true)]
    [InlineData(typeof(byte), true)]
    [InlineData(typeof(short), true)]
    [InlineData(typeof(ushort), true)]
    [InlineData(typeof(int), true)]
    [InlineData(typeof(uint), true)]
    [InlineData(typeof(long), true)]
    [InlineData(typeof(ulong), true)]
    [InlineData(typeof(float), true)]
    [InlineData(typeof(double), true)]
    [InlineData(typeof(decimal), true)]
    [InlineData(typeof(char), false)]
    [InlineData(typeof(bool), false)]
    [InlineData(typeof(int?), false)]
    [InlineData(typeof(string), false)]
    public void TypeIsNumber_ForEveryNumberType(Type type, bool expected)
    {
        type.TypeIsNumber().ShouldBe(expected);
    }

    [Fact]
    public void IsNumber_ForNumberValues()
    {
        ((object) 1).IsNumber().ShouldBeTrue();
        ((object) 1.5m).IsNumber().ShouldBeTrue();
        ((object) (byte) 1).IsNumber().ShouldBeTrue();
        ((object) 'a').IsNumber().ShouldBeFalse();
        ((object) "1").IsNumber().ShouldBeFalse();
        ((object?) null).IsNumber().ShouldBeFalse();
    }

    [Fact]
    public void GetDefault_IsWhatDefaultGives()
    {
        typeof(int).GetDefault().ShouldBe(0);
        typeof(SKColor).GetDefault().ShouldBe(default(SKColor));
        typeof(bool).GetDefault().ShouldBe(false);
        typeof(string).GetDefault().ShouldBeNull();
        typeof(int?).GetDefault().ShouldBeNull();
    }

    #endregion

    #region Casting

    [Theory]
    [InlineData(typeof(double), typeof(int), true)]
    [InlineData(typeof(int), typeof(double), true)]
    [InlineData(typeof(object), typeof(string), true)]
    [InlineData(typeof(IEnumerable), typeof(string), true)]
    [InlineData(typeof(int), typeof(char), true)]
    [InlineData(typeof(char), typeof(int), false)]
    [InlineData(typeof(string), typeof(int), false)]
    [InlineData(typeof(string), typeof(object), false)]
    public void IsCastableFrom_ForNumbersAssignableTypesAndKnownConversions(Type to, Type from, bool expected)
    {
        to.IsCastableFrom(from).ShouldBe(expected);
    }

    [Fact]
    public void IsCastableFrom_OnlyFindsOperatorsDeclaredOnTheSourceType()
    {
        typeof(Fahrenheit).IsCastableFrom(typeof(Celsius)).ShouldBeTrue();

        // Pinned as-is: Fahrenheit declares a conversion from Kelvin, but only the operators of Kelvin are looked at
        typeof(Fahrenheit).IsCastableFrom(typeof(Kelvin)).ShouldBeFalse();
    }

    [Theory]
    [InlineData(typeof(int), typeof(int), 5)]
    [InlineData(typeof(double), typeof(int), 4)]
    [InlineData(typeof(int), typeof(double), 4)]
    [InlineData(typeof(int), typeof(char), 3)]
    [InlineData(typeof(object), typeof(string), 1)]
    [InlineData(typeof(string), typeof(int), 0)]
    [InlineData(typeof(string), null, 0)]
    public void ScoreCastability_RatesHowWellTypesConvert(Type to, Type? from, int expected)
    {
        to.ScoreCastability(from).ShouldBe(expected);
    }

    [Fact]
    public void ScoreCastability_RatesOperatorsAboveInheritance()
    {
        typeof(Fahrenheit).ScoreCastability(typeof(Celsius)).ShouldBe(2);
        typeof(GenericBase<int>).ScoreCastability(typeof(DerivedFromGeneric)).ShouldBe(1);
    }

    #endregion

    #region Enumerables and generics

    [Theory]
    [InlineData(typeof(List<int>), true)]
    [InlineData(typeof(IEnumerable<int>), true)]
    [InlineData(typeof(int[]), true)]
    [InlineData(typeof(Dictionary<string, int>), true)]
    [InlineData(typeof(ArrayList), false)]
    [InlineData(typeof(int), false)]
    [InlineData(typeof(string), false)]
    public void IsGenericEnumerable_ForGenericCollectionsButNotText(Type type, bool expected)
    {
        type.IsGenericEnumerable().ShouldBe(expected);
    }

    [Theory]
    [InlineData(typeof(List<int>), typeof(int))]
    [InlineData(typeof(IEnumerable<string>), typeof(string))]
    [InlineData(typeof(SKColor[]), typeof(SKColor))]
    [InlineData(typeof(Dictionary<string, int>), typeof(KeyValuePair<string, int>))]
    [InlineData(typeof(ArrayList), null)]
    [InlineData(typeof(int), null)]
    public void GetGenericEnumerableType_GivesTheItemType(Type type, Type? expected)
    {
        type.GetGenericEnumerableType().ShouldBe(expected);
    }

    [Fact]
    public void GetGenericEnumerableType_OfTextIsCharacters()
    {
        // Unlike IsGenericEnumerable this doesn't make an exception for text
        typeof(string).GetGenericEnumerableType().ShouldBe(typeof(char));
    }

    [Theory]
    [InlineData(typeof(List<int>), typeof(List<>), true)]
    [InlineData(typeof(List<int>), typeof(IEnumerable<>), true)]
    [InlineData(typeof(List<>), typeof(List<>), true)]
    [InlineData(typeof(DerivedTwice), typeof(GenericBase<>), true)]
    [InlineData(typeof(int[]), typeof(IReadOnlyList<>), true)]
    [InlineData(typeof(List<int>), typeof(Dictionary<,>), false)]
    [InlineData(typeof(string), typeof(List<>), false)]
    [InlineData(typeof(object), typeof(List<>), false)]
    public void IsOfGenericType_LooksAtBaseTypesAndInterfaces(Type type, Type genericType, bool expected)
    {
        type.IsOfGenericType(genericType).ShouldBe(expected);
    }

    [Fact]
    public void IsOfGenericType_NeedsAGenericDefinition()
    {
        Should.Throw<ArgumentException>(() => typeof(List<int>).IsOfGenericType(typeof(List<int>)));
    }

    [Theory]
    [InlineData(typeof(int), "int")]
    [InlineData(typeof(string), "string")]
    [InlineData(typeof(object), "object")]
    [InlineData(typeof(int?), "int?")]
    [InlineData(typeof(SKColor), "SKColor")]
    [InlineData(typeof(List<string>), "List<string>")]
    [InlineData(typeof(Dictionary<string, List<int?>>), "Dictionary<string, List<int?>>")]
    public void GetDisplayName_UsesKeywordsAndReadableGenerics(Type type, string expected)
    {
        type.GetDisplayName().ShouldBe(expected);
    }

    [Theory]
    [InlineData(typeof(ColorGradient), "Color gradient")]
    [InlineData(typeof(List<ColorGradient>), "List<Color gradient>")]
    [InlineData(typeof(int), "Int")]
    public void GetDisplayName_CanBeHumanized(Type type, string expected)
    {
        type.GetDisplayName(true).ShouldBe(expected);
    }

    [Fact]
    public void GetDisplayName_OfAnArrayLosesItsBracketsWhenHumanized()
    {
        typeof(ColorGradient[]).GetDisplayName().ShouldBe("ColorGradient[]");

        // Pinned as-is: humanizing drops the brackets, so a list of gradients reads the same as a single gradient
        typeof(ColorGradient[]).GetDisplayName(true).ShouldBe("Color gradient");
    }

    #endregion
}
