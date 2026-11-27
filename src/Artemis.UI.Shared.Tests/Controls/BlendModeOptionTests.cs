using SkiaSharp;

namespace Artemis.UI.Shared.Tests.Controls;

public class BlendModeOptionTests
{
    [Fact]
    public void All_ContainsEveryBlendModeOnce()
    {
        List<FolderBlendMode> listed = BlendModeOption.All.Where(o => !o.IsHeader).Select(o => o.Value!.Value).ToList();

        listed.ShouldBe(Enum.GetValues<FolderBlendMode>(), ignoreOrder: true);
    }

    [Fact]
    public void All_DescribesEveryBlendMode()
    {
        foreach (BlendModeOption option in BlendModeOption.All.Where(o => !o.IsHeader))
            option.Description.ShouldNotBeNullOrWhiteSpace(option.Name);
    }

    [Fact]
    public void All_StartsWithPassThroughAndNormal()
    {
        BlendModeOption.All[0].Value.ShouldBe(FolderBlendMode.PassThrough);
        BlendModeOption.All[1].Value.ShouldBe(FolderGeneralProperties.FromSKBlendMode(SKBlendMode.SrcOver));
    }
}
