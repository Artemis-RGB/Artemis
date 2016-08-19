using Artemis.Profiles.Layers.Animations;
using Artemis.Profiles.Layers.Conditions;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Types.Audio;
using Artemis.Profiles.Layers.Types.Folder;
using Artemis.Profiles.Layers.Types.Generic;
using Artemis.Profiles.Layers.Types.Headset;
using Artemis.Profiles.Layers.Types.Keyboard;
using Artemis.Profiles.Layers.Types.KeyboardGif;
using Artemis.Profiles.Layers.Types.KeyPress;
using Artemis.Profiles.Layers.Types.Mouse;
using Ninject.Modules;

namespace Artemis.InjectionModules
{
    public class ProfileModules : NinjectModule
    {
        public override void Load()
        {
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
            Bind<ILayerType>().To<KeyPressType>();
            Bind<ILayerType>().To<AudioType>();

            // Bind some Layer Types to self as well in order to allow JSON.NET injection
            Bind<KeyPressType>().ToSelf();
            Bind<AudioType>().ToSelf();
        }
    }
}