# Arduino Installation


## 1. Download and install Arduino
For Mac: https://www.arduino.cc/en/main/software

For Windows: https://www.microsoft.com/en-us/p/arduino-ide/9nblggh4rsd8?ocid=badge&rtc=1



## 2. Install Arduino Core for ESP8266 WiFi Chip

GitHub link: https://github.com/esp8266/Arduino

Open
```
File > Preferences
```

You can find `Additional Boards Manager URLs`

Paste this link in the dialog:
```
https://arduino.esp8266.com/stable/package_esp8266com_index.json
```

Open
```
Tools > Board: “Arduino Uno” > Boards Manager...
```

Type and Search `ESP`

Install `esp8266 by ESP8266 Community`


## 3. Compile and Upload the Empty Arduino Code

Choose
```
Tools > Board: “Arduino Uno” > ESP8266
```

Select `LOLIN (WEMOS) D1 R2 & mini`

Plug ESP8266 Board with micro USB cable

Select
```
Tools > Port > COM3 (Windows)
or
Tools > Port > /dev/cu.usb-xxxx (Mac)
```

Upload the code to see if it works


## 4. Setup WiFi username and password

Open arduino directory of the project

Check the computer’s local IP address

Windows: Open Terminal and type `ipconfig` then find
```
Wireless LAN adapter Wi-Fi > IPv4 Address (e.g., 10.0.0.68)
```

Mac:
```
System Preferences > Network > Wi-Fi is connected to XXX and has the IP address 10.0.0.68.
```

Duplicate `config.h.example` file and rename it to `config.h`

Change
```
- ssid = “your-wifi-name"
- password = “your-wifi-password"
- nodeIP = “10.0.0.68” (the IP address you got above)
```


## 5. Install dependent Arduino libraries

### Install ArduinoJson

Open
```
Tools > Manage Libraries
```

Type `ArduinoJson`

Make sure to select version `5.x` (e.g., 5.13.5), instead of `6.x`

Install ArduinoJson library 5.x

### Install RotaryEncoder

Open
```
Tools > Manage Libraries
```

Type `RotaryEncoder`

Install RotaryEncoder by Matthias Hertel


## 6. Compile and upload the Arduino code to ESP test board

Now you should be able to compile and upload the code to ESP8255 board

Press Upload button

Open the serial monitor

You will see something like
```
WiFi connected
IP address: 10.0.020.8
```

This is ESP8266 chip’s IP address


## 7. Plug the robot’s ESP board and upload the code

Now, let’s upload the same code to a robot’s ESP board

Plug the micro-USB cable to the top part

Make sure the white line of magnet is vertically aligned with the slit (eye)

THIS IS VERY IMPORTANT STEP

> We are using TX/RX as GPIO pins because of the lack of available pins, and TX/RX should be LOW when uploading or restarting the ESP board.
>
> Therefore, the rotary encoder should be placed to a specific location
>
> I have identified the position and put the white electric tape as a guiding line
>
> Rotate the magnet with a screwdriver or something to appropriate position

Prese be careful not to peel off the white tape

Once the magnetic pole is appropriately positioned, now you can upload

Again, select the appropriate board, port

Then, compile and upload the code

Open the serial monitor to check the robot’s IP address and remember it

You need the step 3, when uploading the code or restarting the robot by clicking the reset button. (e.g., when the battery is running out, etc)

