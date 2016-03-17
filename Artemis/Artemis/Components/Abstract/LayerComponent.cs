using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Artemis.Models;
using Artemis.Models.Interfaces;

namespace Artemis.Components.Abstract
{
    public abstract class LayerComponent
    {
        public string Name { get; set; }
        public List<LayerConditionModel> ConditionModels { get; set; }

        public bool ConditionsMet(IGameDataModel dataModel)
        {
            return ConditionModels.All(cm => cm.ConditionMet(dataModel));
        }

        public abstract void Draw(Graphics g);
    }
}