using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Artemis.Core;

namespace Artemis.VisualScripting.Editor.Controls
{
    public class VisualScriptEditor : Control
    {
        #region Dependency Properties

        public static readonly DependencyProperty ScriptProperty = DependencyProperty.Register(
            "Script", typeof(INodeScript), typeof(VisualScriptEditor), new PropertyMetadata(default(INodeScript)));

        public INodeScript Script
        {
            get => (INodeScript)GetValue(ScriptProperty);
            set => SetValue(ScriptProperty, value);
        }

        public static readonly DependencyProperty AvailableNodesProperty = DependencyProperty.Register(
            "AvailableNodes", typeof(IEnumerable<NodeData>), typeof(VisualScriptEditor), new PropertyMetadata(default(IEnumerable<NodeData>)));

        public IEnumerable<NodeData> AvailableNodes
        {
            get => (IEnumerable<NodeData>)GetValue(AvailableNodesProperty);
            set => SetValue(AvailableNodesProperty, value);
        }

        public static readonly DependencyProperty AlwaysShowValuesProperty = DependencyProperty.Register(
            "AlwaysShowValues", typeof(bool), typeof(VisualScriptEditor), new PropertyMetadata(default(bool)));

        public bool AlwaysShowValues
        {
            get => (bool)GetValue(AlwaysShowValuesProperty);
            set => SetValue(AlwaysShowValuesProperty, value);
        }

        #endregion
    }
}
