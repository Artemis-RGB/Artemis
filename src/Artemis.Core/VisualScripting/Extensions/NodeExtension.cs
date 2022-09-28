using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Core;

/// <summary>
///     Provides extension methods for nodes.
/// </summary>
public static class NodeExtension
{
    #region Methods

    /// <summary>
    ///     Estimates a height of a node in the editor.
    /// </summary>
    /// <param name="node">The node whose height to estimate.</param>
    /// <returns>The estimated height in pixels.</returns>
    public static double EstimateHeight(this INode node)
    {
        const double PIN_HEIGHT = 26;
        const double TITLE_HEIGHT = 46;

        int inputPinCount = node.Pins.Count(x => x.Direction == PinDirection.Input)
                            + node.PinCollections.Where(x => x.Direction == PinDirection.Input).Sum(x => x.Count() + 1);
        int outputPinCount = node.Pins.Count(x => x.Direction == PinDirection.Output)
                             + node.PinCollections.Where(x => x.Direction == PinDirection.Output).Sum(x => x.Count() + 1);

        return TITLE_HEIGHT + Math.Max(inputPinCount, outputPinCount) * PIN_HEIGHT;
    }

    /// <summary>
    ///     Estimates a width a node in the editor.
    /// </summary>
    /// <param name="node">The node whose width to estimate.</param>
    /// <returns>The estimated width in pixels.</returns>
    public static double EstimateWidth(this INode node)
    {
        // DarthAffe 13.09.2022: For now just assume they are all the same size
        return 120;
    }

    /// <summary>
    ///     Determines whether the node is part of a loop when the provided pending connecting would be connected.
    /// </summary>
    /// <param name="node">The node to check</param>
    /// <param name="pendingConnection">The node to which a connection is pending</param>
    /// <returns><see langword="true" /> if there would be a loop; otherwise <see langword="false" />.</returns>
    public static bool IsInLoop(this INode node, INode pendingConnection)
    {
        HashSet<INode> checkedNodes = new();

        bool CheckNode(INode checkNode, INode? pending)
        {
            if (checkedNodes.Contains(checkNode)) return false;

            checkedNodes.Add(checkNode);

            List<INode> connectedNodes = checkNode.Pins
                .Where(x => x.Direction == PinDirection.Input)
                .SelectMany(x => x.ConnectedTo)
                .Select(x => x.Node)
                .Concat(checkNode.PinCollections
                    .Where(x => x.Direction == PinDirection.Input)
                    .SelectMany(x => x)
                    .SelectMany(x => x.ConnectedTo)
                    .Select(x => x.Node))
                .Distinct()
                .ToList();
            if (pending != null)
                connectedNodes.Add(pending);

            foreach (INode connectedNode in connectedNodes)
            {
                if (connectedNode == node)
                    return true;
                else if (CheckNode(connectedNode, null))
                    return true;
            }

            return false;
        }

        return CheckNode(node, pendingConnection);
    }

    #endregion
}