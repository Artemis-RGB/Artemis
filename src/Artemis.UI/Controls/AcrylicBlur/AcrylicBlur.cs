using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Artemis.UI.Controls.AcrylicBlur;

public class AcrylicBlur : ContentControl
{
    private static readonly ImmutableExperimentalAcrylicMaterial DefaultAcrylicMaterial = (ImmutableExperimentalAcrylicMaterial) new ExperimentalAcrylicMaterial()
    {
        MaterialOpacity = 0.1,
        TintColor = new Color(255, 7, 7, 7),
        TintOpacity = 1,
        PlatformTransparencyCompensationLevel = 0
    }.ToImmutable();

    public static readonly StyledProperty<ExperimentalAcrylicMaterial?> MaterialProperty =
        AvaloniaProperty.Register<AcrylicBlur, ExperimentalAcrylicMaterial?>(nameof(Material));

    public ExperimentalAcrylicMaterial? Material
    {
        get => GetValue(MaterialProperty);
        set => SetValue(MaterialProperty, value);
    }

    public static readonly StyledProperty<int> BlurProperty = AvaloniaProperty.Register<AcrylicBlur, int>(nameof(Blur));

    public int Blur
    {
        get => GetValue(BlurProperty);
        set => SetValue(BlurProperty, value);
    }

    static AcrylicBlur()
    {
        AffectsRender<AcrylicBlur>(MaterialProperty);
        AffectsRender<AcrylicBlur>(BlurProperty);
    }

    public override void Render(DrawingContext context)
    {
        ImmutableExperimentalAcrylicMaterial mat = Material != null ? (ImmutableExperimentalAcrylicMaterial) Material.ToImmutable() : DefaultAcrylicMaterial;
        context.Custom(new AcrylicBlurRenderOperation(this, mat, Blur, new Rect(default, Bounds.Size)));
    }
}