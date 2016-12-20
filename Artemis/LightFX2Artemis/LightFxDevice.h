#pragma once
#include "LightFxLight.h"
#include "json.hpp"

using json = nlohmann::json;

class LightFxDevice
{
public:
	LightFxDevice();
	~LightFxDevice();
	void SetLightFromInt(int lightIndex, const unsigned colorVal);
	json GetJson();
	LightFxLight* Lights[5];
};
