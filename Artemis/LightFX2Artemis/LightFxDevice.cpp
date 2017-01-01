#include "LightFxDevice.h"
using json = nlohmann::json;


LightFxDevice::LightFxDevice()
{
	for (int i = 0; i < 6; i++)
	{
		Lights[i] = new LightFxLight();
	}
}


LightFxDevice::~LightFxDevice()
{
}

void LightFxDevice::SetLightFromInt(int lightIndex, const unsigned colorVal)
{
	Lights[lightIndex]->FromInt(colorVal);
}

json LightFxDevice::GetJson()
{
	json j;
	j["lights"] = {};
	for (LightFxLight* light : Lights)
	{
		j["lights"].push_back(light->GetJson());
	}

	return j;
}
