# Artemis
Artemis adds support for several games to a range of RGB keyboards.

Besides game-support there are also a few effects for when you're not gaming. 

**Download**: https://github.com/SpoinkyNL/Artemis/releases

**FAQ**: https://github.com/SpoinkyNL/Artemis/wiki/Frequently-Asked-Questions-(FAQ)

Some of it's basic features:

 * Support for multiple keyboards.
 * Support for multiple games.
 * A few non-gaming effects such as type waves and audio visualization.
 * A fancy, modern looking UI.


Currently the following games are supported:

 * Rocket League (uses memory reading for now, we've had contact with Psyonix for a better solution)
 * The Witcher 3 (uses a mod)
 * Counter-Strike Global Offensive (uses native gamestate intergration)

Support is planned for:
 * Dota 2 (in development right now!)
 * Project CARS (using native memory sharing)
 * What you happen to suggest!

For online games we greatly perfer to use an official API, since memory reading is frowned upon by anti-cheat sofware.

The following keyboards are supported:
 * Corsair K95 RGB
 * Corsair K70 RGB
 * Corsair K65 RGB (Untested)
 * Corsair Strafe RGB
 * Logitech G910
 * Logitech G810 (untested)
 * Logitech G410 (untested)

Razer BlackWidow Chroma support coming soon.

For any keyboards/games/effects we'd love PRs!

### Video
A quick demo of Rocket League support in the old codebase (a better video demonstrating all functionality will be put up before release)

[![RocketLeague](http://img.youtube.com/vi/L8rqFGaPeTg/0.jpg)](https://www.youtube.com/watch?v=L8rqFGaPeTg "Rocket League")


A demo of The Witcher 3 support on the new codebase

[![Witcher3](http://img.youtube.com/vi/H03D_y2cFYs/0.jpg)](https://www.youtube.com/watch?v=H03D_y2cFYs "The Witcher 3")


### Screenshots
![Welcome Screen](http://i.imgur.com/hug66P4.png)
![Flyout](http://i.imgur.com/6NTAuYR.png)
![Debug](http://i.imgur.com/uGzRhxP.png)
![Audio Visualization](http://i.imgur.com/Q0C50fe.png)
![Type Waves](http://i.imgur.com/BhLWThq.png)
![Witcher](http://i.imgur.com/IkQuJ6m.png)
![Dota](http://i.imgur.com/kc6sHjE.png)
![CS:GO](http://i.imgur.com/Eg1ASem.png)
![Rocket League](http://i.imgur.com/L7qilNp.png)
![Volume Overlay](http://i.imgur.com/bS9NhfB.png)


### Thanks to:

 * [CUE.NET](https://github.com/DarthAffe/CUE.NET) - Corsair keyboard library
 * [Colore](https://github.com/CoraleStudios/Colore) - Razer keyboard library
 * [NAudio](http://naudio.codeplex.com/) - Used for audio visualization
 * [Open.WinKeyboardHook](Open.WinKeyboardHook) - Used for typing effects/volume overlay
 * [Caliburn.Micro](http://caliburnmicro.com/) - Awesome WPF framework, really, try it out
 * [MahApps.Metro](https://github.com/MahApps/MahApps.Metro) - Sweet Metro UI
 * [Extended.Wpf.Toolkit](http://wpftoolkit.codeplex.com/) - Color Picker
