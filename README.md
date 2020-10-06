# Kiosk Annunciator Button
Read departures when the annunciator button is pressed on a kiosk.

[![Build Status](https://dev.azure.com/cumtd/MTD/_apis/build/status/CUMTD.KioskAnnunciatorButton?branchName=master)](https://dev.azure.com/cumtd/MTD/_build/latest?definitionId=14&branchName=master)

## Pre Requisites

Install the [SeaMAX software and drivers](https://www.sealevel.com/support/software-seamax-windows/) on the target PC.

## Logging

Logging is provided by [Serilog](https://serilog.net/).\
The application is configured with three logging targets.

* Console (Only for debugging)
* File (Files can be found in `/Logs`)
* [Seq](https://datalust.co/seq)

Logging is configured in `appsettings.json`

## Projects

### KioskAnnunciatorButton.Annunciator
This project handles actually reading out the departures. It uses [Azure Cognitive Speech Services](https://azure.microsoft.com/en-us/services/cognitive-services/speech-services/).

### KioskAnnunciatorButton.RealTime
This project handles the send/recieve of realtime info from the main kiosk server.

### KioskAnnunciatorButton.SeaLevelReader
This project handles reading button presses from the [SeaLevel reader](https://www.sealevel.com/product/8113-usb-to-4-isolated-inputs-digital-interface-adapter/).

### KioskAnnunciatorButton.WorkerService
The main control service. This should be installed as a windows service in production.
