
# Welcome to the **Artemis API documentation**
On this website you can browse the Artemis Core and Shared UI API.
A large part of this documentation is being generated based on code but over time the plan is to expand the documentation with written guides.


## Plugins
Artemis 2.0 has been developed from the ground up with plugins in mind. This means almost all functionality can be expanded. The following plugin types are currently available and fully implemented:
 - [DataModelExpansion\<T\>](api/Artemis.Core.Plugins.DataModelExpansions.DataModelExpansion-1.html)
 - [DeviceProvider](api/Artemis.Core.Plugins.DeviceProviders.DeviceProvider.html)
 - [LayerBrush\<T\>](api/Artemis.Core.Plugins.LayerBrushes.LayerBrush-1.html)
	 - [PerLedLayerBrush\<T\>](api/Artemis.Core.Plugins.LayerBrushes.PerLedLayerBrush-1.html)
	 - [RgbNetLayerBrush\<T\>](api/Artemis.Core.Plugins.LayerBrushes.RgbNetLayerBrush-1.html)
 - [LayerEffect](api/Artemis.Core.Plugins.LayerEffects.LayerEffect-1.html)
 - [Module](api/Artemis.Core.Plugins.Modules.Module.html)
	 - [Module\<T\>](api/Artemis.Core.Plugins.Modules.Module-1.html)
 - [ProfileModule](api/Artemis.Core.Plugins.Modules.ProfileModule.html), 
	 - [ProfileModule\<T\>](api/Artemis.Core.Plugins.Modules.ProfileModule-1.html)

These allow you to expand on Artemis's functionality. For quick and interactive plugin creation, use the [Visual Studio template extension](https://marketplace.visualstudio.com/items?itemName=SpoinkyNL.ArtemisTemplates).  

Example implementations of these plugins can be found on [GitHub](https://github.com/Artemis-RGB/Artemis/tree/master/src/Plugins).

## Services
Artemis provides plugins with an API through a range of services.  
All the services are available to plugins by using dependency injection in your plugin's constructor. Dependency injection is also available for the different view models plugins may provide.

- [Core Services](/api/Artemis.Core.Services.Interfaces.html)
- [UI Services](/api/Artemis.UI.Shared.Services.Interfaces.html)

