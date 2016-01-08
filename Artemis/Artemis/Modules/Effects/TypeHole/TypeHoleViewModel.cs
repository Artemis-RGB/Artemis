using Artemis.Events;
using Artemis.Models;
using Caliburn.Micro;

namespace Artemis.Modules.Effects.TypeHole
{
    public class TypeHoleViewModel : Screen, IHandle<ChangeActiveEffect>
    {
        public TypeHoleViewModel(MainModel mainModel)
        {
            // Subscribe to main model
            MainModel = mainModel;
            MainModel.Events.Subscribe(this);

            // Create effect model and add it to MainModel
            TypeHoleModel = new TypeHoleModel();
            MainModel.EffectModels.Add(TypeHoleModel);
        }

        public MainModel MainModel { get; set; }
        public TypeHoleModel TypeHoleModel { get; set; }

        public static string Name => "Type Holes (NYI)";
        public bool EffectEnabled => MainModel.IsEnabled(TypeHoleModel);

        public void Handle(ChangeActiveEffect message)
        {
            NotifyOfPropertyChange(() => EffectEnabled);
        }

        public void ToggleEffect()
        {
            MainModel.EnableEffect(TypeHoleModel);
        }
    }
}