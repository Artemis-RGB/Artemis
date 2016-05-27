#define _RZCHROMASDK_H_

#pragma once

#include "RzChromaSDKTypes.h"

using namespace ChromaSDK;
using namespace Keyboard;
using namespace Mouse;
using namespace Headset;
using namespace Mousepad;
using namespace Keypad;

// Exported functions
#ifdef __cplusplus
extern "C"
{
#endif

	RZRESULT Init(void);

	RZRESULT UnInit(void);

	RZRESULT CreateEffect(RZDEVICEID DeviceId, ChromaSDK::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId);

	RZRESULT CreateKeyboardEffect(Keyboard::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId);

	RZRESULT CreateMouseEffect(Mouse::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId);

	RZRESULT CreateHeadsetEffect(Headset::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId);

	RZRESULT CreateMousepadEffect(Mousepad::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId);

	RZRESULT CreateKeypadEffect(Keypad::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId);

	RZRESULT DeleteEffect(RZEFFECTID EffectId);

	RZRESULT SetEffect(RZEFFECTID EffectId);

	RZRESULT RegisterEventNotification(HWND hWnd);

	RZRESULT UnregisterEventNotification();

	RZRESULT QueryDevice(RZDEVICEID DeviceId, DEVICE_INFO_TYPE& DeviceInfo);

#ifdef __cplusplus
}
#endif
