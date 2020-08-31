

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
**Documentation**: https://artemis-rgb.com/docs/

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

For an up-to-date overview of what's currently being worked on, see the [Projects](https://github.com/SpoinkyNL/Artemis/projects) page

## Plugin development
While Artemis 2 is still in development, the plugin API is pretty far along.  
To get started, you can download the Visual Studio extension in the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=SpoinkyNL.ArtemisTemplates).

This extension provides you with interactive template projects for each type of Artemis 2 plugin.
To use the templates you must have Artemis 2 extracted somewhere on your drive. The plugin wizard will ask you to locate the exectuable in order to setup all the required project references for you.

Due to the volatine nature of the project right now, there is no documentation yet. The templates provide some commentary to get you going and feel free to ask for more help in Discord.

## Work in progress screenshots
**Note:** Video tutorials and written guides on many of the features displayed below are planned for when Artemis 2 nears feature-completion.

![Plugins](https://i.imgur.com/x8LGJxp.png)
_Artemis 2 has been build from the ground up with modularity in mind. This means almost everything can be extended using plugins. New devices, effects, games and almost everything else can be added through plugins._

![Surface editor](https://i.imgur.com/uA8JLL5.png)
_The surface editor allows you to recreate your desktop in 2D space, this provides Artemis with spatial awareness and ensures effects scale properly over your different devices. Right clicking a device lets you change its properties such as rotation and scale._

![Profile editor](https://i.imgur.com/PydFspu.png)
_Here is an overview of the profile editor. While it may be overwhelming at first it is very simple to get some basic effects set up. When you're ready, the profile editor will allow you to create almost anything you can think of._

![LED selection](https://i.imgur.com/7DM0c1x.png)
_Layers are created by making a selection of LEDs, this allows you to precisely dictate where a layer may render._

![Shapes](https://i.imgur.com/NRzc5B1.png)
_Inside the layer you can freely manipulate the shape that is being rendered. By default it always fills the entire layer as seen in the previous screenshot, but it can shrink and even be a circle, revealing the rainbow background behind._

![Keyframes](http://artemis-rgb.com/github/sSEvdAXeTQ.gif)
_With the keyframe engine you can animate almost any property of the layer. In the example above the position and scale of the shape have been animated using keyframes._

![Conditions](https://i.imgur.com/ERHRFQj.png)
_Using visual programming you can create conditions to dictate when a layer may show. The data available to these conditions is provided by plugins, allowing easy expansion._
