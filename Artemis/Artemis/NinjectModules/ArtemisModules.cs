using Artemis.Modules.Effects.AudioVisualizer;
using Artemis.Modules.Effects.Debug;
using Artemis.Modules.Effects.TypeWave;
using Artemis.Modules.Games.CounterStrike;
using Artemis.Modules.Games.Dota2;
using Artemis.Modules.Games.RocketLeague;
using Artemis.Modules.Games.TheDivision;
using Artemis.Modules.Games.Witcher3;
using Artemis.Modules.Overlays.VolumeDisplay;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;
using Ninject.Modules;

namespace Artemis.NinjectModules
{
    public class ArtemisModules : NinjectModule
    {
        public override void Load()
        {
            // Effects
            Bind<Screen>().To<EffectViewModel>(); // TODO: Needed?
            Bind<EffectViewModel>().To<AudioVisualizerViewModel>().InSingletonScope();
            Bind<EffectViewModel>().To<DebugEffectViewModel>().InSingletonScope();
            Bind<EffectViewModel>().To<TypeWaveViewModel>().InSingletonScope();
            //Bind<EffectViewModel>().To<AmbientLightningEffectViewModel>().InSingletonScope();

            // Games
            Bind<Screen>().To(typeof(GameViewModel<>)); // TODO: Needed?
            Bind<GameViewModel<CounterStrikeDataModel>>().To<CounterStrikeViewModel>().InSingletonScope();
            Bind<GameViewModel<Dota2DataModel>>().To<Dota2ViewModel>().InSingletonScope();
            Bind<GameViewModel<RocketLeagueDataModel>>().To<RocketLeagueViewModel>().InSingletonScope();
            Bind<GameViewModel<TheDivisionDataModel>>().To<TheDivisionViewModel>().InSingletonScope();
            Bind<GameViewModel<Witcher3DataModel>>().To<Witcher3ViewModel>().InSingletonScope();

            // Overlays
            Bind<Screen>().To<OverlayViewModel>(); // TODO: Needed?
            Bind<OverlayViewModel>().To<VolumeDisplayViewModel>().InSingletonScope();
        }
    }
}