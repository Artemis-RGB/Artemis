using MaterialDesignThemes.Wpf;
using Ninject;
using Stylet;

namespace Artemis.UI.Screens.Sidebar
{
    public class SidebarScreenViewModel<T> : SidebarScreenViewModel where T : Screen
    {
        public SidebarScreenViewModel(PackIconKind icon, string displayName) : base(icon, displayName)
        {
        }

        public override Screen CreateInstance(IKernel kernel)
        {
            return kernel.Get<T>();
        }
    }

    public abstract class SidebarScreenViewModel
    {
        protected SidebarScreenViewModel(PackIconKind icon, string displayName)
        {
            Icon = icon;
            DisplayName = displayName;
        }

        public abstract Screen CreateInstance(IKernel kernel);
        public PackIconKind Icon { get; }
        public string DisplayName { get; }
    }
}