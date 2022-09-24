using System.Collections.Generic;
using System.Linq;

namespace Artemis.Core;

public static class NodeScriptExtension
{
    #region Methods

    public static void Organize(this NodeScript nodeScript)
    {
        const double SPACING_HORIZONTAL = 160;
        const double SPACING_VERTICAL = 20;

        Dictionary<INode, int> levels = nodeScript.Nodes.ToDictionary(node => node, _ => -1);

        List<INode> currentLevelNodes = nodeScript.Nodes.Where(x => x.IsExitNode).ToList();
        foreach (INode currentLevelNode in currentLevelNodes)
            levels[currentLevelNode] = 0; // DarthAffe 13.09.2022: Init-exit nodes as zero

        int currentLevel = 1;
        while (currentLevelNodes.Count > 0 && currentLevel < 1000)
        {
            List<INode> nextLevelNodes = currentLevelNodes.SelectMany(node => node.Pins
                                                                                  .Where(x => x.Direction == PinDirection.Input)
                                                                                  .SelectMany(x => x.ConnectedTo)
                                                                                  .Select(x => x.Node)
                                                                                  .Concat(node.PinCollections
                                                                                              .Where(x => x.Direction == PinDirection.Input)
                                                                                              .SelectMany(x => x)
                                                                                              .SelectMany(x => x.ConnectedTo)
                                                                                              .Select(x => x.Node)))
                                                          .Distinct()
                                                          .ToList();

            foreach (INode nextLevelNode in nextLevelNodes)
                if (currentLevel > levels[nextLevelNode])
                    levels[nextLevelNode] = currentLevel;

            currentLevelNodes = nextLevelNodes;
            currentLevel++;
        }

        void LayoutLevel(IList<INode> nodes, double posX, double posY)
        {
            foreach (INode node in nodes)
            {
                node.X = posX;
                node.Y = posY;

                posY += SPACING_VERTICAL + node.EstimateHeight();
            }
        }

        List<INode>? unusedNodes = null;
        double unusedPosY = 0;
        double level0Width = 0;

        double positionX = 0;
        foreach (IGrouping<int, INode> levelGroup in levels.GroupBy(x => x.Value, x => x.Key).OrderBy(x => x.Key))
        {
            List<INode> nodes = levelGroup.ToList();
            double levelHeight = nodes.Sum(x => x.EstimateHeight()) + ((nodes.Count - 1) * SPACING_VERTICAL);
            double levelWidth = nodes.Max(x => x.EstimateWidth());
            double positionY = -(levelHeight / 2.0);

            if (levelGroup.Key == -1)
            {
                unusedNodes = nodes;
                unusedPosY = positionY;
            }
            else
            {
                if (levelGroup.Key == 0)
                    level0Width = levelWidth;

                LayoutLevel(nodes, positionX, positionY);

                positionX -= SPACING_HORIZONTAL + levelWidth;
            }
        }

        if (unusedNodes != null)
            LayoutLevel(unusedNodes, level0Width + (SPACING_HORIZONTAL / 2.0), unusedPosY);
    }

    #endregion
}