#include "LightFxLight.h"


LightFxLight::LightFxLight()
{
	Color = new LFX_COLOR{0,0,0,0};
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

void LightFxLight::FromInt(const unsigned colorVal)
{
	Color->brightness = (colorVal >> 24) & 0xFF;
	Color->red = (colorVal >> 16) & 0xFF;
	Color->green = (colorVal >> 8) & 0xFF;
	Color->blue = colorVal & 0xFF;
}
