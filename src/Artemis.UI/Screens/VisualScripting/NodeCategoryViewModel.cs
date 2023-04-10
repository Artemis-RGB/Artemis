using System.Collections.Generic;
using System.Linq;
using Artemis.Core;

namespace Artemis.UI.Screens.VisualScripting;

public class NodeCategoryViewModel
{
    public NodeCategoryViewModel(DynamicData.List.IGrouping<NodeData,string> category)
    {
        Category = category.Key;
        Nodes = category.Items.ToList();
    }

    public string Category { get; set; }
    public List<NodeData> Nodes { get; set; }
}