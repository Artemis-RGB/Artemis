using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using FuzzySharp.Extractor;

namespace Artemis.UI.Screens.VisualScripting;

public class NodeDataViewModel : ViewModelBase
{
    private int _searchScore;

    public NodeDataViewModel(NodeData nodeData)
    {
        NodeData = nodeData;
    }

    public NodeData NodeData { get; }

    public int SearchScore
    {
        get => _searchScore;
        set => RaiseAndSetIfChanged(ref _searchScore, value);
    }

    public void SetSearchScore(List<ExtractedResult<string>> scoredByName, List<ExtractedResult<string>> scoredByDescription)
    {
        int nameScore = scoredByName.FirstOrDefault(s => s.Value == NodeData.Name)?.Score ?? -1;
        int descriptionScore = (scoredByDescription.FirstOrDefault(s => s.Value == NodeData.Description)?.Score ?? -1) / 3;
        SearchScore = Math.Max(nameScore, descriptionScore);
    }
}