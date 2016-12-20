#include "LightFxState.h"
#include "json.hpp"

using json = nlohmann::json;

LightFxState::LightFxState(char* game)
{
	Game = game;
	for (int i = 0; i < 5; i++)
	{
		Devices[i] = new LightFxDevice();
	}
	LocationMaskLight = new LightFxLight();
}


LightFxState::~LightFxState()
{
}

json LightFxState::GetJson()
{
	json root;
	json j;
	root["lightFxState"] = { j };

	j["game"] = Game;
	j["mask"] = {
		{ "location", LocationMask },
		{ "light", LocationMaskLight->GetJson() }
	};
	j["devices"] = {};
	for (LightFxDevice* device : Devices)
	{
		j["devices"].push_back(device->GetJson());
	}

	return j;
}
