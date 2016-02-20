# Artemis
Artemis adds support for several games to a range of RGB keyboards.

Besides game-support there are also a few effects for when you're not gaming. 

Some of it's basic features:

 * Support for multiple keyboards planned.
 * Support for multiple games.
 * A few non-gaming effects such as type waves and audio visualization.
 * A fancy, modern looking UI.


Currently the following games are supported:

 * Rocket League (uses memory reading for now, we've had contact with Psyonix for a better solution)
 * The Witcher 3 (uses a mod)
 * Counter-Strike Global Offensive (uses native gamestate intergration)

Support is planned for:
 * Dota 2 (using native gamestate intergration))
 * Project CARS (using native memory sharing)
 * What you happen to suggest!

For online games we greatly perfer to use an official API, since memory reading is frowned upon by anti-cheat sofware.

Currently the only supported keyboards are the Logitech G910 Orion Spark and Corsair K95 RGB, but progress is being made to support Razer as well.

For any keyboards/games/effects we'd love PRs!

### Video
A quick demo of Rocket League support in the old codebase (a better video demonstrating all functionality will be put up before release)

[![RocketLeague](http://img.youtube.com/vi/L8rqFGaPeTg/0.jpg)](https://www.youtube.com/watch?v=L8rqFGaPeTg "Rocket League")


A demo of The Witcher 3 support on the new codebase

[![Witcher3](http://img.youtube.com/vi/H03D_y2cFYs/0.jpg)](https://www.youtube.com/watch?v=H03D_y2cFYs "The Witcher 3")


### Screenshots
![Screenshot 1](http://i.imgur.com/7bho4WH.png)
![Screenshot 2](http://i.imgur.com/X7LMaWr.png)
![Screenshot 3](http://i.imgur.com/iSy4jfW.png)
![Screenshot 4](http://i.imgur.com/P7WUsS6.png)
![Screenshot 5](http://i.imgur.com/JNoLaDB.png)


### Thanks to:

 * [CUE.NET](https://github.com/DarthAffe/CUE.NET) - Corsair keyboard library
 * [Colore](https://github.com/CoraleStudios/Colore) - Razer keyboard library
 * [NAudio](http://naudio.codeplex.com/) - Used for audio visualization
 * [Open.WinKeyboardHook](Open.WinKeyboardHook) - Used for typing effects/volume overlay
 * [Caliburn.Micro](http://caliburnmicro.com/) - Awesome WPF framework, really, try it out
 * [MahApps.Metro](https://github.com/MahApps/MahApps.Metro) - Sweet Metro UI
 * [Extended.Wpf.Toolkit](http://wpftoolkit.codeplex.com/) - Color Picker
