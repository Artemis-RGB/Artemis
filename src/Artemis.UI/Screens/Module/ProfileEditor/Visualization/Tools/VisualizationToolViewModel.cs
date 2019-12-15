using System.Windows;
using System.Windows.Input;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization.Tools
{
    public abstract class VisualizationToolViewModel : CanvasViewModel
    {
        protected VisualizationToolViewModel(ProfileViewModel profileViewModel, IProfileEditorService profileEditorService)
        {
            // Not relevant for visualization tools as they overlay the entire canvas
            X = 0;
            Y = 0;

            ProfileViewModel = profileViewModel;
            ProfileEditorService = profileEditorService;
            Cursor = Cursors.Arrow;
        }

        public ProfileViewModel ProfileViewModel { get; }
        public IProfileEditorService ProfileEditorService { get; }
        public Cursor Cursor { get; protected set; }
        public bool IsMouseDown { get; protected set; }
        public Point MouseDownStartPosition { get; protected set; }

        public virtual void MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsMouseDown = true;
            MouseDownStartPosition = e.GetPosition((IInputElement) sender);
        }

        public virtual void MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsMouseDown = false;
        }

        public virtual void MouseMove(object sender, MouseEventArgs e)
        {
        }

        public virtual void MouseWheel(object sender, MouseWheelEventArgs e)
        {
        }
    }
}