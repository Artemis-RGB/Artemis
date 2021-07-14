using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using Artemis.VisualScripting.Editor.Controls.Wrapper;

namespace Artemis.VisualScripting.Editor.Controls
{
    [TemplatePart(Name = PART_PATH, Type = typeof(Path))]
    public class VisualScriptCablePresenter : Control
    {
        #region Constants

        private const string PART_PATH = "PART_Path";

        #endregion

        #region Properties & Fields

        private Path _path;

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty CableProperty = DependencyProperty.Register(
            "Cable", typeof(VisualScriptCable), typeof(VisualScriptCablePresenter), new PropertyMetadata(default(VisualScriptCable)));

        public VisualScriptCable Cable
        {
            get => (VisualScriptCable)GetValue(CableProperty);
            set => SetValue(CableProperty, value);
        }

        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(
            "Thickness", typeof(double), typeof(VisualScriptCablePresenter), new PropertyMetadata(default(double)));

        public double Thickness
        {
            get => (double)GetValue(ThicknessProperty);
            set => SetValue(ThicknessProperty, value);
        }

        #endregion
        
        #region Methods

        public override void OnApplyTemplate()
        {
            _path = GetTemplateChild(PART_PATH) as Path ?? throw new NullReferenceException($"The Path '{PART_PATH}' is missing.");
            _path.MouseDown += OnPathMouseDown;
        }

        private void OnPathMouseDown(object sender, MouseButtonEventArgs args)
        {
            if ((args.ChangedButton == MouseButton.Left) && (args.LeftButton == MouseButtonState.Pressed) && (args.ClickCount == 2))
            {
                //TODO DarthAffe 17.06.2021: Should we add rerouting?
                //AddRerouteNode();
            }
            else if (args.ChangedButton == MouseButton.Middle)
                Cable.Disconnect();
        }

        #endregion
    }
}
