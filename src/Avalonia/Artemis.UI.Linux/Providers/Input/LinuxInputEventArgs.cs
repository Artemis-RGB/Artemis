namespace Artemis.UI.Linux.Providers.Input
{
    //https://www.kernel.org/doc/Documentation/input/input.txt
    internal readonly struct LinuxInputEventArgs
    {
        internal readonly long TimeSeconds;
        internal readonly long TimeMicroseconds;
        internal readonly LinuxInputEventType Type;
        internal readonly short Code;
        internal readonly int Value;
    }
}