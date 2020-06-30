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

**Please note that even though I've included a plugin for each brand supported by RGB.NET, only Corsair and Logitech have been tested. If you run into any issues please let us know on Discord.**

#### Want to build? Follow these instructions
1. Create a central folder like ```C:\Repos```
2. Clone RGB.NET's development branch into ```<central folder>\RGB.NET```
3. Clone Artemis into  ```<central folder>\Artemis```
5. Open ```<central folder>\RGB.NET\RGB.NET.sln``` and build with the default config
4. Open ```<central folder>\Artemis\src\Artemis.sln```
5. Restore Nuget packages

A quick overview of the largest changes compared to Artemis 1.x
- Completely overhauled UI
- Deep intergration with DarthAffe's [RGB.NET](https://github.com/DarthAffe/RGB.NET) (meaning more devices!)
- Custom device positions to accurately map out your real-world setup
- Profiles are no longer be bound to a specific keyboard but will with any setup
- Layers are assigned to LEDs to allow for very precise lighting
- Profiles are built using an entirely custom made editor with support for keyframe-based animations and shape manipulation
- Devices, layer types (known as brushes and effects in Artemis 2), modules and many other things are available in the form of plugins
- There will be a workshop to share plugins, profiles and even layers. This workshop will be integrated into the application and will also part of an Artemis website

Much of this is subject to change and will take a while to create but it'll leave us with a much better platform to create a community around :smiley:
For an up-to-date overview of what's currently being worked on, see the [Projects](https://github.com/SpoinkyNL/Artemis/projects) page

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
