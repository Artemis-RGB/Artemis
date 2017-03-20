//using Artemis.Managers;
//using Artemis.Modules.Abstract;
//using Ninject;
//
//namespace Artemis.Modules.Games.TheDivision
//{
//    public sealed class TheDivisionViewModel : ModuleViewModel
//    {
//        public TheDivisionViewModel(MainManager mainManager, [Named(nameof(TheDivisionModel))] ModuleModel moduleModel,
//            IKernel kernel) : base(mainManager, moduleModel, kernel)
//        {
//            DisplayName = "The Division";
//        }
//
//        public override bool UsesProfileEditor => true;
//    }
//}