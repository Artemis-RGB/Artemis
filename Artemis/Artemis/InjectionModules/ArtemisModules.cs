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
using Ninject.Modules;

namespace Artemis.InjectionModules
{
    public class ArtemisModules : NinjectModule
    {
        public override void Load()
        {
            // Effects
            Bind<EffectViewModel>().To<AudioVisualizerViewModel>().InSingletonScope();
            Bind<EffectViewModel>().To<DebugEffectViewModel>().InSingletonScope();
            Bind<EffectViewModel>().To<TypeWaveViewModel>().InSingletonScope();
            //Bind<EffectViewModel>().To<AmbientLightningEffectViewModel>().InSingletonScope();

            // Games
            Bind<GameViewModel>().To<CounterStrikeViewModel>().InSingletonScope();
            Bind<GameViewModel>().To<Dota2ViewModel>().InSingletonScope();
            Bind<GameViewModel>().To<RocketLeagueViewModel>().InSingletonScope();
            Bind<GameViewModel>().To<TheDivisionViewModel>().InSingletonScope();
            Bind<GameViewModel>().To<Witcher3ViewModel>().InSingletonScope();

            // Overlays
            Bind<OverlayViewModel>().To<VolumeDisplayViewModel>().InSingletonScope();
        }
    }
}