#include "LightFxDevice.h"
using json = nlohmann::json;


LightFxDevice::LightFxDevice()
{	
}


LightFxDevice::~LightFxDevice()
{
}

void LightFxDevice::SetLightFromInt(int lightIndex, const unsigned colorVal)
{	
	Lights[lightIndex].Color->brightness = (colorVal >> 24) & 0xFF;
	Lights[lightIndex].Color->red = (colorVal >> 16) & 0xFF;
	Lights[lightIndex].Color->green = (colorVal >> 8) & 0xFF;
	Lights[lightIndex].Color->blue = colorVal & 0xFF;
}

json LightFxDevice::GetJson()
{
	json j;
	j["lights"] = {};
	for (LightFxLight light : Lights)
	{
		j["lights"].push_back(light.GetJson());
	}

	return j;
}
