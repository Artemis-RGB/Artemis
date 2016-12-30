# Artemis
[![GitHub release](https://img.shields.io/github/release/spoinkynl/Artemis.svg)]()
[![Github All Releases](https://img.shields.io/github/downloads/spoinkynl/artemis/setup.exe.svg)]()
[![GitHub license](https://img.shields.io/badge/license-AGPL-blue.svg)](https://raw.githubusercontent.com/SpoinkyNL/Artemis/master/LICENSE)
[![GitHub stars](https://img.shields.io/github/stars/SpoinkyNL/Artemis.svg)](https://github.com/SpoinkyNL/Artemis/stargazers)
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=VQBAEJYUFLU4J) 

Artemis adds highly configurable support for several games to a range of RGB keyboards, mice and headsets.

**Download**: https://github.com/SpoinkyNL/Artemis/releases

**FAQ**: https://github.com/SpoinkyNL/Artemis/wiki/Frequently-Asked-Questions-(FAQ)

### Current Features
 - Support for Corsair, Logitech and Razer keyboards.
 - Support for multiple games including Dota 2, The Witcher 3, Rocket League, and CS:GO.
 - Non-gaming effects such as type waves and audio visualization.
 - A layer-based profile system allowing an immense amount of customization.
 - A fancy, modern looking UI with multiple themes.
 - LUA scripting engine ([docs in the wiki](https://github.com/SpoinkyNL/Artemis/wiki/LUA)).

##### Currently the following games are supported
 - Counter-Strike Global Offensive (uses native gamestate intergration)
 - Dota 2
 - Euro Truck Simulator 2
 - Overwatch
 - Rocket League
 - The Division (beta)
 - The Witcher 3
 - Unreal Tournament


##### The following keyboards are supported
 - All Corsair RGB keyboards
 - Logitech G910
 - Logitech G810
 - Logitech G410 (untested)
 - Razer BlackWidow Chroma

##### Other peripherals
 - All Corsair RGB mice
 - All Corsair RGB headsets
 - Logitech mice and headsets as generic devices

For online games we greatly prefer to use an official API, since memory reading is frowned upon by anti-cheat sofware.

### Profile editor
For its games Artemis uses a custom-made layer system. For each game you can make one or more profiles consisting of layers that interact with the game. Layers can contain a color, multiple colors, animations and even GIFs!

All this is done in the profile editor. For each game and keyboard we provide a default profile, but you're free to duplicate and modify this, or create one entirely from scratch.

![Profile editor screenshot](http://i.imgur.com/tzc9bpO.png)

The screenshot above shows the profile editor as it would appear for CS:GO. The preview shows a rough indication of how the layers will look and allows dragging and resizing. A realtime preview is also shown on the keyboard.

Layers can be reordered or organized into folders by dragging and dropping them. 
There are multiple layer types

 - Keyboard layer
 - Keyboard GIF layer (Yes, allows you to display a GIF on your keyboard!)
 - Mouse layer
 - Headset layer
 - Folder

A keyboard layer's appearance is controlled by its brush. Brushes can be created using the color picker. For more examples, see the demo profile included in the 'Windows Profile' effect.

![Color picker](http://i.imgur.com/sC6Zua6.png)

The above screenshot shows a rainbow gradient made with the color picker.

![Layer editor conditions](http://i.imgur.com/y7a1GMr.png)

A layer's properties can be adjusted to react to ingame events and variables. This screenshot shows how conditions can be attached to a layer, in this case this layer is only enabled when the player has more than 14.000 ingame cash.

Besides adding display conditions to layers, it is also possible to base certain properties on what is happening ingame. 

![Layer editor dynamic properties](http://i.imgur.com/sJ5Gz0k.png)

In this example, the layer is configured to become smaller when the player loses health. The layer's height is based on the percentage of health left, with the maximum health being 100, shrinking downwards as the health decreases.

To keep things interesting, layers can also be configured to use animations. Here's a preview of the demo profile showing a few of the things possible.

![Animations preview](https://thumbs.gfycat.com/UnlinedAlertBoilweevil-size_restricted.gif)

### Videos
Rocket League boost display

[![RocketLeague](http://img.youtube.com/vi/L8rqFGaPeTg/0.jpg)](https://www.youtube.com/watch?v=L8rqFGaPeTg "Rocket League")


The Witcher 3 sign display

[![Witcher3](http://img.youtube.com/vi/H03D_y2cFYs/0.jpg)](https://www.youtube.com/watch?v=H03D_y2cFYs "The Witcher 3")


### Special thanks to:
 - [Corsair](http://corsair.com) and [GloriousGe0rge](https://twitter.com/GloriousGe0rge) in particular for their continued support of Artemis
 - [DarthAffe](https://github.com/DarthAffe) for his great help and awesome CUE SDK wrapper, [CUE.NET](https://github.com/DarthAffe/CUE.NET)
 - [JewsOfHazard](https://github.com/JewsOfHazard) and [Thoth2020](https://github.com/Thoth2020) for their work and ideas
 - All the people that helped by reporting bugs over the last few months
