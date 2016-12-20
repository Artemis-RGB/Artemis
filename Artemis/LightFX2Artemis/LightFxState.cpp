#include "LightFxState.h"
#include "json.hpp"

using json = nlohmann::json;

LightFxState::LightFxState(char* game)
{
	Game = game;
}


LightFxState::~LightFxState()
{
}

const char* LightFxState::Update()
{
	json j;
	j["game"] = Game;
	j["devices"] = {};
	for (LightFxDevice device : Devices)
	{
		j["devices"].push_back(device.GetJson());
	}

	std::string s = j.dump();
	return s.c_str();
}
