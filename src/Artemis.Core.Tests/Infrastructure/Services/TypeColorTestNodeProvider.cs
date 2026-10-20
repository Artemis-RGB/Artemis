using Artemis.Core.Nodes;
using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.Services;

/// <summary>
///     A node provider that only registers pin type colors, the way node plugins give their pin types a color
/// </summary>
public class TypeColorTestNodeProvider : NodeProvider
{
    public static readonly SKColor NumberColor = new(10, 20, 30);
    public static readonly SKColor TextColor = new(40, 50, 60);

    public override void Enable()
    {
        RegisterTypeColor<Numeric>(NumberColor);
        RegisterTypeColor<string>(TextColor);
    }

    public override void Disable()
    {
    }
}
