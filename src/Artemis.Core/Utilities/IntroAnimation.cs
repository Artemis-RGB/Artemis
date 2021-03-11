using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.Storage.Entities.Profile;
using Serilog;
using SkiaSharp;

namespace Artemis.Core
{
    internal class IntroAnimation
    {
        private readonly ILogger _logger;
        private readonly IProfileService _profileService;
        private readonly IEnumerable<ArtemisDevice> _devices;

        public IntroAnimation(ILogger logger, IProfileService profileService, IEnumerable<ArtemisDevice> devices)
        {
            _logger = logger;
            _profileService = profileService;
            _devices = devices;

            AnimationProfile = CreateIntroProfile();
        }

        public Profile AnimationProfile { get; set; }

        public void Render(double deltaTime, SKCanvas canvas)
        {
            AnimationProfile.Update(deltaTime);
            AnimationProfile.Render(canvas);
        }

        private Profile CreateIntroProfile()
        {
            try
            {
                // Load the intro profile from JSON into a ProfileEntity
                string json = File.ReadAllText(Path.Combine(Constants.ApplicationFolder, "Resources", "intro-profile.json"));
                ProfileEntity profileEntity = CoreJson.DeserializeObject<ProfileEntity>(json)!;
                // Inject every LED on the surface into each layer
                foreach (LayerEntity profileEntityLayer in profileEntity.Layers)
                    profileEntityLayer.Leds.AddRange(_devices.SelectMany(d => d.Leds).Select(l => new LedEntity
                    {
                        DeviceIdentifier = l.Device.Identifier,
                        LedName = l.RgbLed.Id.ToString()
                    }));

                Profile profile = new(new DummyModule(), profileEntity);
                profile.Activate(_devices);

                _profileService.InstantiateProfile(profile);
                return profile;
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Failed to load intro profile");
            }

            return new Profile(new DummyModule(), "Intro");
        }
    }

    internal class DummyModule : ProfileModule
    {
        public override void Enable()
        {
            throw new NotImplementedException();
        }

        public override void Disable()
        {
            throw new NotImplementedException();
        }

        public override void Update(double deltaTime)
        {
            throw new NotImplementedException();
        }

        public override void Render(double deltaTime, SKCanvas canvas, SKImageInfo canvasInfo)
        {
            throw new NotImplementedException();
        }

        public override void ModuleActivated(bool isOverride)
        {
            throw new NotImplementedException();
        }

        public override void ModuleDeactivated(bool isOverride)
        {
            throw new NotImplementedException();
        }
    }
}