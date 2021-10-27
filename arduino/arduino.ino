#include <ESP8266WiFi.h>
#include <WiFiClient.h>
#include <ESP8266WebServer.h>
#include <ESP8266mDNS.h>
#include <WiFiUDP.h>
#include <ArduinoJson.h>
#include <RotaryEncoder.h>
#include "config.h"

WiFiUDP UDP;
IPAddress myIP;

char packetBuffer[255]; 
int localPort = 8883;

int a1 = 5;  // D1
int a2 = 4;  // D2
int b1 = 16; // D0
int b2 = 14; // D5

RotaryEncoder encoderA(1, 3);   // TX, RX
RotaryEncoder encoderB(12, 13); // D6, D7

void setup() {
  pinMode (a1, OUTPUT);
  pinMode (a2, OUTPUT);
  pinMode (b1, OUTPUT);
  pinMode (b2, OUTPUT);

  Serial.begin (9600);
  delay(10);

  Serial.println();
  Serial.print("Connecting to ");
  Serial.println(ssid);
  WiFi.hostname("Name");
  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("");
  Serial.println("WiFi connected");

  // Print the IP address
  Serial.print("IP address: ");
  Serial.print(WiFi.localIP());
  Serial.println();
  Serial.println();

  if (MDNS.begin("esp8266")) {
    Serial.println ("MDNS responder started");
  }

  myIP = WiFi.localIP();
  Serial.print("AP IP address: ");
  Serial.println(myIP);
  UDP.begin(localPort);

  delay(500);
  pinMode(1, FUNCTION_3); // TX: swap the pin to a GPIO 1
  pinMode(3, FUNCTION_3); // RX: swap the pin to a GPIO 3
  RotaryEncoder encoderA(1, 3);   // TX, RX
  RotaryEncoder encoderB(12, 13); // D6, D7
}

int targetPosA = 0;
int targetPosB = 0;
bool actuatingA = false;
bool actuatingB = false;
int directionA = 0; // +1: up, -1: down
int directionB = 0; // +1: up, -1: down

int posA = 0;
int posB = 0;
int currentPosA = 0;
int currentPosB = 0;

void tick() {
  encoderA.tick();
  encoderB.tick();
  RotaryEncoder::Direction dirA = encoderA.getDirection();
  if (dirA == RotaryEncoder::Direction::COUNTERCLOCKWISE) {
    posA = posA - 1;
  } else if (dirA == RotaryEncoder::Direction::CLOCKWISE) {
    posA = posA + 1;
  }
  RotaryEncoder::Direction dirB = encoderB.getDirection();
  if (dirB == RotaryEncoder::Direction::COUNTERCLOCKWISE) {
    posB = posB - 1;
  } else if (dirB == RotaryEncoder::Direction::CLOCKWISE) {
    posB = posB + 1;
  }
  currentPosA = posA;
  currentPosB = posB;
}



void loop() {
  tick();

  char bff[32];
  sprintf(bff, "ip: %d.%d.%d.%d , reel_a: %i , reel_b: %i", myIP[0], myIP[1], myIP[2], myIP[3], posA, posB);
  UDP.beginPacket(nodeIP, localPort);
  UDP.write(bff, strlen(bff));
  //  UDP.write(currentPos);
  UDP.endPacket();

  int statusA = 0; // 0, +1, -1
  int statusB = 0; // 0, +1, -1

  if (actuatingA || actuatingB) {
    for (int i = 0; i< 200; i++) {
      move(statusA, statusB);
      delayMicroseconds(1);
      tick();
      statusA = 0; // 0, +1, -1
      statusB = 0; // 0, +1, -1
      if (currentPosA < targetPosA && directionA > 0) {
        statusA = 1;
      }
      if (currentPosA > targetPosA && directionA < 0) {
        statusA = -1;
      }
      if (currentPosB < targetPosB && directionB > 0) {
        statusB = 1;
      }
      if (currentPosB > targetPosB && directionB < 0) {
        statusB = -1;
      }
      if (statusA == 0 && statusB == 0) {
        break;
      }
    }
    if (statusA == 0) {
      actuatingA = false;
      directionA = 0;
    }
    if (statusB == 0) {
      actuatingB = false;
      directionB = 0;
    }
  } else {
    directionA = 0;
    directionB = 0;
    stop();
  }


  int packetSize = UDP.parsePacket();
  if (packetSize) {
    int len = UDP.read(packetBuffer, packetSize);
    if (len > 0) packetBuffer[len] = '\0';

    String json = packetBuffer;
    StaticJsonBuffer<200> jsonBuffer;
    JsonObject& root = jsonBuffer.parseObject(json);

    Serial.println("receive");
    analogWrite(a1, root["a1"]);
    analogWrite(a2, root["a2"]);
    analogWrite(b1, root["b1"]);
    analogWrite(b2, root["b2"]);

    int ms = root["ms"];
    if (ms > 0) {
      delay(ms);
      analogWrite(a1, 0);
      analogWrite(a2, 0);
      analogWrite(b1, 0);
      analogWrite(b2, 0);
    }

    if (root["init"]) {
      encoderA.setPosition(0);
      encoderB.setPosition(0);
      posA = 0;
      posB = 0;
      currentPosA = 0;
      currentPosB = 0;
    }

    if (root["move"]) {
      targetPosA = root["target_a"];
      targetPosB = root["target_b"];
      actuatingA = true;
      actuatingB = true;
      if (currentPosA < targetPosA) {
        directionA = 1; // go up
      }
      if (currentPosA > targetPosA) {
        directionA = -1; // go down
      }
      if (currentPosB < targetPosB) {
        directionB = 1; // go up
      }
      if (currentPosB > targetPosB) {
        directionB = -1; // go down
      }
    }

    if (root["stop"]) {
      actuatingA = false;
      actuatingB = false;
    }
  }
}


void move(int statusA, int statusB) {
  if (statusA > 0 && statusB > 0) {
    analogWrite(a1, 800);
    analogWrite(a2, 0);
    analogWrite(b1, 800);
    analogWrite(b2, 0);
  }
  if (statusA > 0 && statusB == 0) {
    analogWrite(a1, 800);
    analogWrite(a2, 0);
    analogWrite(b1, 0);
    analogWrite(b2, 0);
  }
  if (statusA > 0 && statusB < 0) {
    analogWrite(a1, 800);
    analogWrite(a2, 0);
    analogWrite(b1, 0);
    analogWrite(b2, 800);
  }
  if (statusA == 0 && statusB > 0) {
    analogWrite(a1, 0);
    analogWrite(a2, 0);
    analogWrite(b1, 800);
    analogWrite(b2, 0);
  }
  if (statusA == 0 && statusB == 0) {
    analogWrite(a1, 0);
    analogWrite(a2, 0);
    analogWrite(b1, 0);
    analogWrite(b2, 0);
  }
  if (statusA == 0 && statusB < 0) {
    analogWrite(a1, 0);
    analogWrite(a2, 0);
    analogWrite(b1, 0);
    analogWrite(b2, 800);
  }
  if (statusA < 0 && statusB > 0) {
    analogWrite(a1, 0);
    analogWrite(a2, 800);
    analogWrite(b1, 800);
    analogWrite(b2, 0);
  }
  if (statusA < 0 && statusB == 0) {
    analogWrite(a1, 0);
    analogWrite(a2, 800);
    analogWrite(b1, 0);
    analogWrite(b2, 0);
  }
  if (statusA < 0 && statusB < 0) {
    analogWrite(a1, 0);
    analogWrite(a2, 800);
    analogWrite(b1, 0);
    analogWrite(b2, 800);
  }
}

void up() {
  analogWrite(a1, 800);
  analogWrite(a2, 0);
  analogWrite(b1, 800);
  analogWrite(b2, 0);
}

void down() {
  analogWrite(a1, 0);
  analogWrite(a2, 800);
  analogWrite(b1, 0);
  analogWrite(b2, 800);
}

void stop() {
  analogWrite(a1, 0);
  analogWrite(a2, 0);
  analogWrite(b1, 0);
  analogWrite(b2, 0);
}
