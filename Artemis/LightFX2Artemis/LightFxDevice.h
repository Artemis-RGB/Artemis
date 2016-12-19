#pragma once
#include "LightFxLight.h"

class LightFxDevice
{
public:
	LightFxDevice();
	~LightFxDevice();
	LightFxLight Lights[128];
};

