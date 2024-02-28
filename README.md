# Mtd.Kiosk.Annunciator

[![.NET Build](https://github.com/CUMTD/Mtd.Kiosk.Annunciator/actions/workflows/build-test.yml/badge.svg)](https://github.com/CUMTD/Mtd.Kiosk.Annunciator/actions/workflows/build-test.yml)
[![CodeQL](https://github.com/CUMTD/Mtd.Kiosk.Annunciator/actions/workflows/codeql.yml/badge.svg)](https://github.com/CUMTD/Mtd.Kiosk.Annunciator/actions/workflows/codeql.yml)

This project contains the code for departure annunciation on MTD's StopWatch Kiosks.
The service listens for button presses and, when a button is pressed, reads upcoming departures aloud.

## Technologies

* [.NET 8][net8]
* [Azure Cognitive Services][speech-docs] - For Performing Text-to-Speech
* [Serilog][serilog] - For Structured Logging
* [SEQ][seq] - For Log Aggregation

## Projects

### Mtd.Kiosk.Annunciator.Core

This project contains core, and generic code for the kiosk annunciator service.

The service uses implementations of the `IButtonReader` interface to detect button presses.
The `IButtonReader` defines a `Start()` and `Stop()` method and an event handler for when a button is pressed.

`BackgroundButtonReader` is an abstract implementation of `IButtonReader` that
uses a `BackgroundWorker` execute the code that detects button presses in the background.

Implementations if `IButtonReader` that wish to inherit from `BackgroundButtonReader`
should implement their detection code in the `DetectButtonPress()` method.
This method should return true when the button is pressed.
The `BackgroundButtonReader` will raise the `ButtonPressed` event when the button is pressed and
call `DetectButtonPress()` in a loop until the `CancelPending` property is set to true.

Implementations of `IButtonReader` are consumed byt the `Mtd.Kiosk.Annunciator.Service.AnnunciatorService`
class for the purpose of detecting button presses.

```mermaid
classDiagram 
	
	class BackgroundButtonReader  {
		<<Abstract>>
		+ Name : string*
		# ButtonPressed : EventHandler?
		# CancelPending : bool
		- BackgroundWorker? _worker
		- bool _isBackgroundWorkerCurrentlyRunning 
		- bool _isDisposed 
		- ILogger~BackgroundButtonReader~ _logger 
		+ DetectButtonPress() Task~bool~
		+ Start() void
		+ Stop() void
		+ Dispose() void
		- BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e) void
		- CreateWorker() BackgroundWorker
		- Dispose(bool disposing) void
	}
		
	class IButtonReader  {
		<<Interface>>
		Name : string
		ButtonPressed : EventHandler?
		Start() void
		Stop() void
	}

	class IDisposable  {
		<<Interface>>
		Dispose() void
	}
	
	BackgroundButtonReader --|> IButtonReader
	BackgroundButtonReader --|> IDisposable
```

The `Departure` object is a simple DTO that represents a departure from a stop.
The `Departure` DTO is the return value for the `IKioskRealTimeClient.GetRealtime` method
and the parameter for the `IAnnunciator.ReadDepartures` method.

```mermaid
classDiagram 
	
	class Departure  {
		+ Name : string
		+ Time : string
		+ Due : bool
		+ Realtime : bool
		+ JoinWord : string
		+ Departure(string name, string time)
	}
```

The `IKioskRealTimeClient` interface defines methods to interface with MTD servers for real-time data.
The SendHeartbeat method sends a heartbeat to the server for backend reporting on the service's health.
An implementation of `IKioskRealTimeClient` is consumed by the `Mtd.Kiosk.Annunciator.Service.AnnunciatorService` class
for the purpose of fetching departures when the button is pressed.

```mermaid
classDiagram 
	
	class IKioskRealTimeClient  {
		<<interface>>
		SendHeartbeat(string kioskId, CancellationToken cancellationToken) Task
		GetRealtime(string kioskId, CancellationToken cancellationToken) Task~IReadOnlyCollection~Departure~?~
	}
```

The `IAnnunciator` interface defines a method to read departures.
An implementation of `IAnnunciator` is consumed by the `Mtd.Kiosk.Annunciator.Service.AnnunciatorService` class
for the purpose of reading departures aloud.

```mermaid
classDiagram 
	
	class IAnnunciator  {
		<<interface>>
		ReadDepartures(string stopName, IReadOnlyCollection<Departure>? departures, CancellationToken cancellationToken) Task
	}
```

### Mtd.Kiosk.Annunciator.Azure

This project contains code that interfaces with Azure's Text-to-Speech service to read departures aloud.

`Mtd.Kiosk.Annunciator.Azure` is an implementation of `IAnnunciator` that reads departures aloud using Azure's Text-to-Speech service.
The Azure text-to-speech service is a part of the [`Microsoft.CognitiveServices.Speech`][nuget-speech] NuGet package.
More information on the Azure text-to-speech service can be found in the [official documentation][speech-docs].

```mermaid
classDiagram 
	
    class IAnnunciator  {
        <<interface>>
        ReadDepartures(string stopName, IReadOnlyCollection<Departure>? departures, CancellationToken cancellationToken) Task
    }

	class AzureAnnunciator  {
        + AzureAnnunciator(IOptions~AzureAnnunciatorConfig~ config, ILogger~AzureAnnunciator~ logger)
		+ ReadDepartures(string stopName, IReadOnlyCollection<Departure>? departures, CancellationToken cancellationToken) Task
		+ ReadError() Task
        - ILogger~AzureAnnunciator~ _logger 
		- SpeechSynthesizer _synth 
		- ReadLine(string text) Task
		- GenerateSsml(string text) string
	}
	class AzureAnnunciatorConfig  {
		+string ConfigSectionName 
		+ SubscriptionKey : string
		+ ServiceRegion : string
	}
	
    

	AzureAnnunciator --|> IAnnunciator
    AzureAnnunciator --* AzureAnnunciatorConfig	
```

### Mtd.Kiosk.Annunciator.Readers.Simple

This project contains simple implementations of the `IButtonReader` interface.
These are mostly meant for testing and example purposes.
This is a good place to start if building a new implementation.

### Mtd.Kiosk.Annunciator.Realtime.UmbracoApi

This project contains an implementation of `IKioskRealTimeClient` that interfaces with the
Umbraco API that powers the kiosk data.

### Mtd.Kiosk.Annunciator.Service

This project contains `Program.cs`, the entry point for the application.
It runs as either a Windows Service or as a Linux Daemon.
It configures the application and starts the `AnnunciatorService` class.
`AnnunciatorService` is a `BackgroundService` that listens for button presses,
fetches the latest departure information, and reads the departures aloud.

A simplified sequence diagram of the `AnnunciatorService` and the application flow is shown below.

```mermaid
sequenceDiagram
    actor U as User
    participant READ as IButtonReader (BackgroundButtonReader)
    participant SER as AnnunciatorService
    participant RT as IKioskRealTimeClient
    participant ANN as IAnnunciator

    activate SER
    SER ->> SER: Startup
    SER -->> READ: Start in New Process using BackgroundWorker
    activate READ
    SER -->> READ: Subscribe to ButtonPressed event handler
    loop Each Button Press
    Note over READ: Detect Button Press using DetectButtonPress method
    U ->> READ: Presses
    READ ->> SER: Raise ButtonPressed event

    SER -->> RT: Fetch Departures
    activate RT
    RT -->> RT: Fetch departures from server
    RT -->> SER: departures
    deactivate RT

    SER -->> ANN: ReadDepartures
    activate ANN
    ANN -->> ANN: Convert departure to audio
    ANN ->> ANN: Output audio through system audio device
    ANN -->> SER: 
    deactivate ANN

    end

    deactivate READ
    deactivate SER
```

In addition to listening for button presses, the applicaiton also sends heartbeats to the central server
on a periodic basis to report on the health of the service.

## Development

To load the project, you need to add the following values to a user-secrets file or as environment variables.

```json
{
  "AzureAnnunciator": {
    "SubscriptionKey": "Your Subscription ID",
    "ServiceRegion": "Your Region"
  },
  "Kiosk": {
    "Id": "The Kiosk GUID to Associate With",
    "Name": "Kiosk Name to Read"
  },
  "Serilog": {
    "WriteTo": [
    {
      "Name": "Seq",
      "Args": {
        "ServerUrl": "SEQ Server URL"
        "ApiKey": "SEQ API Key"
      }
      }
    ]
  }
}

```

[nuget-speech]: https://www.nuget.org/packages/Microsoft.CognitiveServices.Speech
[speech-docs]: https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/overview
[net8]: https://learn.microsoft.com/en-us/dotnet/core/introduction
[serilog]: https://serilog.net/
[seq]: https://datalust.co/seq
