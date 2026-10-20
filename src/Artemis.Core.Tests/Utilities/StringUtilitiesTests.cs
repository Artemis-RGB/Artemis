namespace Artemis.Core.Tests.Utilities;

public class StringUtilitiesTests
{
    [Theory]
    [InlineData("Hello World", "hello-world")]
    [InlineData("my-profile_v2.final", "my-profile-v2-final")]
    [InlineData("  Leading, and trailing.  ", "leading-and-trailing")]
    [InlineData("a -- b", "a-b")]
    [InlineData("C# & .NET!", "c-net")]
    [InlineData("Ünïcödé Çafé", "unicode-cafe")]
    [InlineData("Straße", "strasse")]
    [InlineData("Price: 5€", "price-5")]
    [InlineData("日本", "")]
    [InlineData("", "")]
    public void UrlFriendly_TurnsTitlesIntoLowercaseWordsSeparatedByDashes(string title, string expected)
    {
        StringUtilities.UrlFriendly(title).ShouldBe(expected);
    }

    [Fact]
    public void UrlFriendly_OfNothingIsEmpty()
    {
        StringUtilities.UrlFriendly(null).ShouldBe("");
    }

    [Fact]
    public void UrlFriendly_CutsLongTitlesShort()
    {
        // Pinned as-is: the limit is meant to be 80 characters, but the character at position 80 is still included
        StringUtilities.UrlFriendly(new string('a', 100)).Length.ShouldBe(81);
    }

    [Fact]
    public void UrlFriendly_CountsSkippedCharactersTowardsTheLimit()
    {
        StringUtilities.UrlFriendly(new string('!', 81) + "abc").ShouldBe("");
    }

    [Theory]
    [InlineData('é', "e")]
    [InlineData('É', "e")]
    [InlineData('å', "a")]
    [InlineData('ø', "o")]
    [InlineData('ü', "u")]
    [InlineData('ç', "c")]
    [InlineData('ž', "z")]
    [InlineData('š', "s")]
    [InlineData('ñ', "n")]
    [InlineData('ÿ', "y")]
    [InlineData('ğ', "g")]
    [InlineData('ř', "r")]
    [InlineData('ł', "l")]
    [InlineData('đ', "d")]
    [InlineData('ß', "ss")]
    [InlineData('Þ', "th")]
    [InlineData('ĥ', "h")]
    [InlineData('ĵ', "j")]
    [InlineData('€', "")]
    [InlineData('a', "")]
    public void RemapInternationalCharToAscii_GivesTheClosestPlainLetter(char c, string expected)
    {
        StringUtilities.RemapInternationalCharToAscii(c).ShouldBe(expected);
    }

    [Theory]
    [InlineData('Ř')]
    [InlineData('Ł')]
    [InlineData('Đ')]
    [InlineData('þ')]
    public void RemapInternationalCharToAscii_OnlyKnowsOneCaseOfSomeLetters(char c)
    {
        // Pinned as-is: most letters are lowercased before they are looked up, but ř, ł, đ, þ, ß, ĥ and ĵ are compared
        // as typed, so their other case isn't recognized. For the thorn only the uppercase Þ is known
        StringUtilities.RemapInternationalCharToAscii(c).ShouldBe("");
    }
}
