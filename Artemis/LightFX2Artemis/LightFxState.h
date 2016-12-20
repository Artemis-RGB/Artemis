#pragma once
#include "LightFxDevice.h"

class LightFxState
{
public:
	LightFxState(char* game);
	~LightFxState();
	const char* Update();
	char* Game;
	LightFxDevice Devices[5];
};
