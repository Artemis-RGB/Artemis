namespace Artemis.Core.Tests.Utilities;

public class EnumUtilitiesTests
{
    private enum PlainEnum
    {
        FirstValue,
        SecondValue
    }

    [Fact]
    public void GetAllValuesAndDescriptions_HumanizesEveryValue()
    {
        EnumUtilities.GetAllValuesAndDescriptions<PlainEnum>().ShouldBe([(PlainEnum.FirstValue, "First value"), (PlainEnum.SecondValue, "Second value")]);
    }

    [Fact]
    public void GetAllValuesAndDescriptions_ByTypeHumanizesEveryValue()
    {
        EnumUtilities.GetAllValuesAndDescriptions(typeof(PlainEnum)).ShouldBe([(PlainEnum.FirstValue, "First value"), (PlainEnum.SecondValue, "Second value")]);
    }

    [Fact]
    public void GetAllValuesAndDescriptions_UsesDescriptionsBothWhenGivenTheTypeAsGenericArgumentAndAsType()
    {
        (KeyboardSection, string) generic = EnumUtilities.GetAllValuesAndDescriptions<KeyboardSection>().Single(v => v.Item1 == KeyboardSection.LedStrips);
        (Enum, string) byType = EnumUtilities.GetAllValuesAndDescriptions(typeof(KeyboardSection)).Single(v => v.Item1.Equals(KeyboardSection.LedStrips));

        generic.Item2.ShouldBe("LED Strips");
        byType.Item2.ShouldBe("LED Strips");
    }

    [Fact]
    public void GetAllValuesAndDescriptions_ByTypeOnlyAcceptsEnums()
    {
        Should.Throw<ArgumentException>(() => EnumUtilities.GetAllValuesAndDescriptions(typeof(int)));
    }

    [Fact]
    public void HumanizeValue_HumanizesTheName()
    {
        EnumUtilities.HumanizeValue(PlainEnum.SecondValue).ShouldBe("Second value");
        EnumUtilities.HumanizeValue(KeyboardSection.MacroKeys).ShouldBe("Macro keys");
    }
}
