using Artemis.DeviceProviders;
using Artemis.DeviceProviders.Corsair;
using Artemis.DeviceProviders.Logitech;
using Artemis.DeviceProviders.Razer;
using Artemis.Modules.Effects.AudioVisualizer;
using Artemis.Modules.Effects.Bubbles;
using Artemis.Modules.Effects.TypeWave;
using Artemis.Modules.Effects.WindowsProfile;
using Artemis.Modules.Games.CounterStrike;
using Artemis.Modules.Games.Dota2;
using Artemis.Modules.Games.Overwatch;
using Artemis.Modules.Games.RocketLeague;
using Artemis.Modules.Games.TheDivision;
using Artemis.Modules.Games.Witcher3;
using Artemis.Modules.Overlays.VolumeDisplay;
using Artemis.Profiles.Layers.Animations;
using Artemis.Profiles.Layers.Conditions;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Types.Folder;
using Artemis.Profiles.Layers.Types.Generic;
using Artemis.Profiles.Layers.Types.Headset;
using Artemis.Profiles.Layers.Types.Keyboard;
using Artemis.Profiles.Layers.Types.KeyboardGif;
using Artemis.Profiles.Layers.Types.Mouse;
using Artemis.ViewModels.Abstract;
using Ninject.Modules;

namespace Artemis.InjectionModules
{
    public class ArtemisModules : NinjectModule
    {
        public override void Load()
        {
            #region Modules

            // Effects
            Bind<EffectViewModel>().To<AudioVisualizerViewModel>().InSingletonScope();
            Bind<EffectViewModel>().To<TypeWaveViewModel>().InSingletonScope();
            Bind<EffectViewModel>().To<BubblesViewModel>().InSingletonScope();
            Bind<EffectViewModel>().To<WindowsProfileViewModel>().InSingletonScope();

            // Games
            Bind<GameViewModel>().To<CounterStrikeViewModel>().InSingletonScope();
            Bind<GameViewModel>().To<Dota2ViewModel>().InSingletonScope();
            Bind<GameViewModel>().To<RocketLeagueViewModel>().InSingletonScope();
            Bind<GameViewModel>().To<TheDivisionViewModel>().InSingletonScope();
            Bind<GameViewModel>().To<Witcher3ViewModel>().InSingletonScope();
            Bind<GameViewModel>().To<OverwatchViewModel>().InSingletonScope();

            // Overlays
            Bind<OverlayViewModel>().To<VolumeDisplayViewModel>().InSingletonScope();

            #endregion

            #region Devices

            // Keyboards
            Bind<DeviceProvider>().To<CorsairKeyboards>().InSingletonScope();
            Bind<DeviceProvider>().To<G910>().InSingletonScope();
            Bind<DeviceProvider>().To<G810>().InSingletonScope();
            Bind<DeviceProvider>().To<BlackWidow>().InSingletonScope();
            // Mice
            Bind<DeviceProvider>().To<CorsairMice>().InSingletonScope();
            // Headsets
            Bind<DeviceProvider>().To<CorsairHeadsets>().InSingletonScope();
            // Other
            Bind<DeviceProvider>().To<LogitechGeneric>().InSingletonScope();

            #endregion

            #region Layers

            // Animations
            Bind<ILayerAnimation>().To<NoneAnimation>();
            Bind<ILayerAnimation>().To<GrowAnimation>();
            Bind<ILayerAnimation>().To<PulseAnimation>();
            Bind<ILayerAnimation>().To<SlideDownAnimation>();
            Bind<ILayerAnimation>().To<SlideLeftAnimation>();
            Bind<ILayerAnimation>().To<SlideRightAnimation>();
            Bind<ILayerAnimation>().To<SlideUpAnimation>();
            // Conditions
            Bind<ILayerCondition>().To<DataModelCondition>();
            Bind<ILayerCondition>().To<EventCondition>();
            // Types
            Bind<ILayerType>().To<FolderType>();
            Bind<ILayerType>().To<HeadsetType>();
            Bind<ILayerType>().To<KeyboardType>();
            Bind<ILayerType>().To<KeyboardGifType>();
            Bind<ILayerType>().To<MouseType>();
            Bind<ILayerType>().To<GenericType>();

            #endregion
        }
    }
}