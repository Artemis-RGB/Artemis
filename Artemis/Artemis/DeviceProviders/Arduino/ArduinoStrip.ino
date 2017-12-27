#include "FastLED.h"//https://github.com/FastLED/FastLED
FASTLED_USING_NAMESPACE
#if defined(FASTLED_VERSION) && (FASTLED_VERSION < 3001000)
#warning "Requires FastLED 3.1 or later; check github for latest code."
#endif

#define DATA_PIN    3 //change to your pin #
//#define CLK_PIN   4 //some leds need a clock
#define LED_TYPE    WS2812B //change to your type of led, check link for supported leds https://github.com/FastLED/FastLED/wiki/Chipset-reference
#define COLOR_ORDER GRB //leds RGB order, WS2812B is GRB
#define NUM_LEDS    288 //change to you number of leds
#define SERIAL_BUFFER_SIZE 2048 //this is really huge but it works so...
CRGB leds[NUM_LEDS];
#define BRIGHTNESS  20 //0-255  at 5V each WS2812B chip draws 50mA so my 288 strip will draw 14.4A thats why I'm limiting to 8% or 5.6W
byte buff[3];

void setup() {
  Serial.begin(2000000);//use buad rate of choose, native USB device will just use max baud rate 
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
    leds[(i)] = CRGB(buff[1], buff[0], buff[2]);//Artemis sends colors as RGB and WS2812B is GRB so I need to change order
  }
}
