#pragma once
#include "LightFxDevice.h"

class LightFxState
{
public:	
	LightFxState(char* game);
	~LightFxState();
	void Update();
	char* Game;
	LightFxDevice Devices[5];
};
