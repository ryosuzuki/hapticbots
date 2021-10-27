# Node.js Server

## Setup

Duplicate `config/default.json.example` as `config/default.json`
Change ip addresses, based on your environments

## Installation for Mac

For Mac users, the script basically should work with the built-in Bluetooth.

But if you get `XpcConnection`-related errors, better to use Terminal.
If you are using iTerm2, toio.js may not work for some reasons. If you get that error, please try to use and run with Terminal instead.
https://github.com/toio/toio.js/issues/22


## Installation for Windows

### 1. Install node v10.x (NOT the latest v12.x) on Powershell
Download from https://nodejs.org/dist/latest-v10.x/ (e.g., node-v10.21.0-x64.msi)

### 2. Install git for Windows
Download from https://git-scm.com/download/win


### 3. Install Python 2.x with the following
Open the Powershell on Visual Studio or Windows Terminal as `administrator`, then run the following command

```
npm install --global windows-build-tools
```

Now, you can npm install noble and bluetooth-hci-socket

But, if you get the following error. Maybe, your laptop's bluetooth module is not compatible with https://github.com/noble/node-bluetooth-hci-socket. Then, go to the next step

```
PS C:\Users\t-rysuz\Documents\GitHub\swarm-display> node .\server.js
listening on 3000
start
C:\Users\t-rysuz\Documents\GitHub\swarm-display\node_modules\bluetooth-hci-socket\lib\usb.js:70
    throw new Error('No compatible USB Bluetooth 4.0 device found!');
    ^

Error: No compatible USB Bluetooth 4.0 device found!
    at BluetoothHciSocket.bindUser (C:\Users\t-rysuz\Documents\GitHub\swarm-display\node_modules\bluetooth-hci-socket\lib\usb.js:70:11)
    at BluetoothHciSocket.bindRaw (C:\Users\t-rysuz\Documents\GitHub\swarm-display\node_modules\bluetooth-hci-socket\lib\usb.js:28:8)
    at Hci.init (C:\Users\t-rysuz\Documents\GitHub\swarm-display\node_modules\noble\lib\hci-socket\hci.js:101:35)
    at NobleBindings.init (C:\Users\t-rysuz\Documents\GitHub\swarm-display\node_modules\noble\lib\hci-socket\bindings.js:82:13)
    at Noble.<anonymous> (C:\Users\t-rysuz\Documents\GitHub\swarm-display\node_modules\noble\lib\noble.js:57:24)
    at process._tickCallback (internal/process/next_tick.js:61:11)
    at Function.Module.runMain (internal/modules/cjs/loader.js:834:11)
    at startup (internal/bootstrap/node.js:283:19)
    at bootstrapNodeJSCore (internal/bootstrap/node.js:623:3)
```

### 4. Get the compatible Bluetooth dongle
TOIO (`bluetooth-hci-socket`) requires compatible bluetooth modules.
I would recommend ASUS AT-4000 on Amazon https://www.amazon.com/dp/B00DJ83070 but please look `bluetooth-hci-socket` repository for more information


### 5. Replace the Bluetooth driver to WinUSB with Zadig tools
see the screenshots

If you made a mistake to select the driver in Zadig tools, you can fix it later from `Device Manager`


### 6. Test if Toio moves

Install the dependency
```
npm install
```

Run `test-move.js`
```
node test-move.js
```

Press arrow key to see if the TOIO moves.

### 6. Run the server

```
node server.js
```

open http://localhost:3000/ in browser

If you can successfully see the position of TOIO in console and web browser, that's success.


### 7. Open Unity
Open `oculus-toio-control` in unity folder


### 8. Run ngrok

```sh
ngrok http 3000
```

Run in the background

```sh
ngrok http 3000 --log=stdout > ngrok.log &
```

To kill

```sh
ps aux | grep ngrok
kill -9 [pid]
``

