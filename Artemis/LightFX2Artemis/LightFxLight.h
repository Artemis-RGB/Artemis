#pragma once
#include "includes/LFXDecl.h"
#include "json.hpp"

using json = nlohmann::json;

class LightFxLight
{
public:
	LightFxLight();
	~LightFxLight();
	json GetJson();
	PLFX_COLOR Color;
};
