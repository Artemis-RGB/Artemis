using Stylet;

namespace Artemis.UI.Stylet
{
    public class ArtemisViewManager : ViewManager
    {
        public ArtemisViewManager(ViewManagerConfig config) : base(config)
        {
        }

        protected override string ViewTypeNameForModelTypeName(string modelTypeName)
        {
            string cleaned = modelTypeName.Split('`')[0];
            return base.ViewTypeNameForModelTypeName(cleaned);
        }
    }
}