using MoonSharp.Interpreter;

namespace Artemis.Modules.Games.LightFx.Data
{
    [MoonSharpUserData]
    public class LightFxState
    {
        public Device[] devices { get; set; }
        public string game { get; set; }
        public Mask mask { get; set; }
    }

    [MoonSharpUserData]
    public class Device
    {
        public Light[] lights { get; set; }
    }

    [MoonSharpUserData]
    public class Mask
    {
        public Light light { get; set; }
        public int location { get; set; }
    }

    [MoonSharpUserData]
    public class Light
    {
        public Color color { get; set; }
    }

    [MoonSharpUserData]
    public class Color
    {
        public int brightness { get; set; }
        public int red { get; set; }
        public int green { get; set; }
        public int blue { get; set; }
    }
}