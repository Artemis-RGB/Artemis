using DryIoc;

namespace Artemis.UI.Shared;

internal static class UI
{
    public static IContainer Locator { get; set; } = null!;
}