#pragma once
#include "LFXDecl.h"
#include "json.hpp"

using json = nlohmann::json;

class LightFxLight
{
public:
	LightFxLight();
	~LightFxLight();
	json GetJson();
	void FromInt(const unsigned colorVal);
	LFX_COLOR* Color;
};
