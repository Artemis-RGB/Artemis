# Artemis
[![Build status](https://build.rbeekman.nl/app/rest/builds/buildType:(id:Artemis_Build)/statusIcon.svg)](https://build.rbeekman.nl/viewType.html?buildTypeId=Artemis_Build&guest=1)
[![GitHub release](https://img.shields.io/github/release/spoinkynl/Artemis.svg)](https://github.com/SpoinkyNL/Artemis/releases)
[![Github All Releases](https://img.shields.io/github/downloads/spoinkynl/artemis/setup.exe.svg)](https://github.com/SpoinkyNL/Artemis/releases)
[![GitHub license](https://img.shields.io/badge/license-GPL3-blue.svg)](https://github.com/SpoinkyNL/Artemis/blob/master/LICENSE)
[![GitHub stars](https://img.shields.io/github/stars/SpoinkyNL/Artemis.svg)](https://github.com/SpoinkyNL/Artemis/stargazers)
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=VQBAEJYUFLU4J) 

Artemis adds highly configurable support for several games to a range of RGB keyboards, mice and headsets.  
**Download**: https://github.com/SpoinkyNL/Artemis/releases  
**FAQ**: https://github.com/SpoinkyNL/Artemis/wiki/Frequently-Asked-Questions-(FAQ)

### Notice
Artemis 2 is in development. In the meanwhile I'm no longer supporting Artemis 1.x, sorry!  
Executable of latest Artemis 2 build: [Artemis_Build.zip]( https://build.rbeekman.nl/repository/downloadAll/Artemis_Build/.lastSuccessful/artifacts.zip?guest=1). To use, extract the ZIP and run Artemis.UI.exe.  

**Please note that for now I've only included Corsair and very limited Logitech support. If you want to test support for a different device (must be RGB.NET supported), let me know on Discord.**

#### Want to build? Follow these instructions
1. Create a central folder like ```C:\Repos```
2. Clone RGB.NET's development branch into ```<central folder>\RGB.NET```
3. Clone Artemis into  ```<central folder>\Artemis```
5. Open ```<central folder>\RGB.NET\RGB.NET.sln``` and build with the default config
4. Open ```<central folder>\Artemis\src\Artemis.sln```
5. Restore Nuget packages
6. Run ```Update-Package -reinstall PropertyChanged.Fody``` on both the **Artemis.Core** and **Artemis.UI** projects

Some things I have planned for 2.0
- Completely overhauled UI
- Deep intergration with DarthAffe's [RGB.NET](https://github.com/DarthAffe/RGB.NET) (meaning more devices!)
- Profiles will no longer be bound to a specific keyboard but will work on any keyboard
- Layers will be assigned to keys accurately like done in per example Corsair's CUE thanks to RGB.NET
- Instead of the built-in game support Artemis currently offers there will be plugins that add support for games, new layers, effects and more
- The plugin system will replace the current idea of having a scripting engine and will be in multiple languages (these will be in the form of plugins themselves)
- I'll include some plugins by default to support most of the games Artemis currently supports (I might drop some games that don't work well/are too much work to maintain)
- There will be a workshop to share plugins, profiles and even layers. This workshop will be part of an Artemis website

Much of this is subject to change and will take a while to create but it'll leave us with a much better platform to create a community around :smiley:

### Work in progress screenshots
![Surface editor](https://i.imgur.com/7d8wgkr.png)
_The surface editor allows you to recreate your desktop in 2D space, ensure effects scale properly over your different devices. Right clicking a device lets you identify it (it will blink white) and change its properties such as rotation and scale._

![Profile editor](https://i.imgur.com/e41dal0.png)
_Here is an overview of the profile editor, the panels aren't populated yet but will allow you to configure display conditions, layer elements (such as colors, animations, filters) and element properties where you can configure the different elements._

![LED selection](https://i.imgur.com/3uYi7Jt.png)
_Inside the profile editor you'll be able to select LEDs and create layers for them or apply them to layers. A layer will only display on the LEDs it is assigned to._

![Rotation and scale](https://i.imgur.com/jen9nbi.png)
_An example of what the profile editor might look like whith a rotated keyboard and an enlarged mouse._

![Settings](https://i.imgur.com/rK4oJ5A.png)
_This is an early version of the device settings tab, in here you can configure devices where applicable._
