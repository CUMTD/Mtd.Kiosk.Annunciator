# Kiosk Annunciator Button
Read departures when the annunciator button is pressed on a kiosk.

[![Build Status](https://dev.azure.com/cumtd/KioskButton/_apis/build/status/CUMTD.KioskAnnunciatorButton)](https://dev.azure.com/cumtd/KioskButton/_build/latest?definitionId=6)

## Installation
1. Build the `Cumtd.Signage.Kiosk.KioskButton` project in release mode.
2. Copy the output of `/KioskButton/bin/x86/Release/` to a folder at the root directory of the target PC.
3. Update the id in `ButtonConfig.json` to match the GUID of the kiosk from https://kiosk.mtd.org/umbraco/.
4. Install the [SeaMAX software and drivers](https://www.sealevel.com/support/software-seamax-windows/) on the target PC.
5. Reboot.
5. In an elevated comand prompt, run `Cumtd.Signage.Kiosk.KioskButton.exe install`.
6. Plug in button USB cable.

## Projects

### Cumtd.Signage.Kiosk.KioskButton
This is the main service. It runs as a hidden console application.

### Cumtd.Signage.Kiosk.Annunciator
Handles the text to speach.

### Cumtd.Signage.Kiosk.RealTime
Handles the fetching and conversion of real-time information.

### Cumtd.Signage.Kiosk.SeaLevel
Handles the interaction with the SeaLevel Sea DAC button.


## Hotkeys
When the applicaiton is running, the following hotkeys can be use:

| Keys                           | Result                |
|--------------------------------|-----------------------|
| `CTRL` + `ALT` + `SHIFT` + `C` | Toggle Console Window |
| `CTRL` + `ALT` + `SHIFT` + `Q` | Exit Application      |
