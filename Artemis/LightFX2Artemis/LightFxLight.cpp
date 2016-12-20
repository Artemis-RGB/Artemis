#include "LightFxLight.h"


LightFxLight::LightFxLight()
{
}


LightFxLight::~LightFxLight()
{
}

json LightFxLight::GetJson()
{
	json j;
	j["color"] = {
		{"red", Color->red},
		{"green", Color->green},
		{"blue", Color->blue},
		{"brightness", Color->brightness}
	};
	return j;
}
