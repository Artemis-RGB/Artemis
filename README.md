

# Artemis
[![Build Status](https://dev.azure.com/artemis-rgb/Artemis/_apis/build/status/Artemis%20Development%20build?repoName=Artemis-RGB%2FArtemis&branchName=master)](https://dev.azure.com/artemis-rgb/Artemis/_build/latest?definitionId=1&repoName=Artemis-RGB%2FArtemis&branchName=master)
[![GitHub release](https://img.shields.io/github/release/spoinkynl/Artemis.svg)](https://github.com/SpoinkyNL/Artemis/releases)
[![GitHub license](https://img.shields.io/badge/license-GPL3-blue.svg)](https://github.com/SpoinkyNL/Artemis/blob/master/LICENSE)
[![GitHub stars](https://img.shields.io/github/stars/SpoinkyNL/Artemis.svg)](https://github.com/SpoinkyNL/Artemis/stargazers)
[![Discord](https://img.shields.io/discord/392093058352676874?logo=discord&logoColor=white)](https://discord.gg/S3MVaC9) 
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=VQBAEJYUFLU4J) 

Artemis 2 adds highly configurable support for several games to a range of RGB keyboards, mice and headsets.  
Artemis 1 is no longer supported and Artemis 2 is in active development. This entire readme and all websites/documents refer to Artemis 2. 

### Check out our [Wiki](https://wiki.artemis-rgb.com) and more specifically, the [getting started guide](https://wiki.artemis-rgb.com/en/guides/user/introduction).
**Pre-release download**: https://github.com/SpoinkyNL/Artemis/releases (pre-release means your profiles may break at any given time!)  
**Plugin documentation**: https://artemis-rgb.com/docs/

**Please note that even though we have plugins for each brand supported by RGB.NET, they have not been thoroughly tested. If you run into any issues please let us know on Discord.**

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

### Third party plugins
A few people have already started working on plugins! If you want your plugins to be added to this list feel free to PR or ask on Discord.
- https://github.com/diogotr7/Artemis.Plugins
- https://github.com/Wibble199/Artemis.Plugins
- https://github.com/F-Lehmann/MyArtemisPlugins

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

### Special thanks
Over the years several companies have supported Artemis by providing both hardware and software, thank you!
[![Corsair](https://i.imgur.com/UKUdDOy.png)](https://www.corsair.com/) 
[![JetBrains](https://i.imgur.com/JYfXjjB.png)](https://www.jetbrains.com/?from=ArtemisRGB) 
[![Wooting](https://i.imgur.com/Zh3bVza.png)](https://wooting.io/) 
