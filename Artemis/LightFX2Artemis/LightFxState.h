#pragma once
#include "LightFxDevice.h"
#include "json.hpp"

using json = nlohmann::json;

class LightFxState
{
public:
	LightFxState(char* game);
	~LightFxState();
	json GetJson();
	char* Game;
	LightFxDevice* Devices[5];
	unsigned LocationMask;
	LightFxLight* LocationMaskLight;
};
