using Stylet;

namespace Artemis.UI.Extensions
{
    public static class ScreenExtensions
    {
        public static T FindScreenOfType<T>(this Screen screen) where T : Screen
        {
            Screen parent = screen.Parent as Screen;
            while (parent != null)
            {
                if (parent is T match)
                    return match;
                parent = parent.Parent as Screen;
            }

            return default;
        }
    }
}