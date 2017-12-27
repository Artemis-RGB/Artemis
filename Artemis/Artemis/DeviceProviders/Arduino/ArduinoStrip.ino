#include "FastLED.h"//https://github.com/FastLED/FastLED
FASTLED_USING_NAMESPACE
#if defined(FASTLED_VERSION) && (FASTLED_VERSION < 3001000)
#warning "Requires FastLED 3.1 or later; check github for latest code."
#endif

#define DATA_PIN    3
//#define CLK_PIN   4
#define LED_TYPE    WS2812B
#define COLOR_ORDER GRB
#define NUM_LEDS    288
#define SERIAL_BUFFER_SIZE 2048 //this is really huge but it works so...
CRGB leds[NUM_LEDS];
#define BRIGHTNESS  20 //0-255  each WS2812B chip draws 0.3W so my 288 strip will draw 86.4Watts thats why I'm limiting to 8% or 6.8W at 5V
byte buff[3];

void setup() {
  Serial.begin(2000000);
  FastLED.addLeds<LED_TYPE, DATA_PIN, COLOR_ORDER>(leds, NUM_LEDS);
  FastLED.setBrightness(BRIGHTNESS);
  pinMode(LED_BUILTIN_TX, INPUT);//disable TX LED for Arduino pro micro
  pinMode(LED_BUILTIN_RX, INPUT);//disable RX LED for Arduino pro micro
  while (!Serial); // wait for serial port to connect. Needed for native USB port only
}

void loop() {
  FastLED.show();
  for (int i = 0; i < NUM_LEDS; i++) {
    Serial.readBytes(buff, 3);
    leds[(i)] = CRGB(buff[1], buff[0], buff[2]);
  }
}
