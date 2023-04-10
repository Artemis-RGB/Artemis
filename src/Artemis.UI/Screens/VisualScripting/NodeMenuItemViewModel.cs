using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Artemis.Core;
using ReactiveUI;

namespace Artemis.UI.Screens.VisualScripting;

public class NodeMenuItemViewModel
{
    public NodeMenuItemViewModel(ReactiveCommand<NodeData, Unit> createNode, DynamicData.List.IGrouping<NodeData, string> category)
    {
        Header = category.Key;
        Items = category.Items.Select(d => new NodeMenuItemViewModel(createNode, d)).ToList();
    }

    public NodeMenuItemViewModel(ReactiveCommand<NodeData, Unit> createNode, NodeData nodeData)
    {
        Header = nodeData.Name;
        Items = new List<NodeMenuItemViewModel>();
        CreateNode = ReactiveCommand.Create(() => { createNode.Execute(nodeData).Subscribe(); });
    }

    public string Header { get; }
    public List<NodeMenuItemViewModel> Items { get; }
    public ReactiveCommand<Unit, Unit>? CreateNode { get; }
}