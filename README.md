RivalTracker-for-iRacing
========================
Written and maintained by [Sam Hazim](https://twitter.com/SamHazim)

RivalTracker is a JavaScript library for embedding track circuit maps onto websites.  The library project is hosted at http://github.com/SamHazim/RivalTracker.

This project is an example implementation of RivalTracker using a custom iRacing SDK C# client, allowing for iRacing session updates from external websites.

This application opens up the iRacing telemetry SDK by running a WebSockets server internally, allowing external client(s) access (e.g. JavaScript).

The C# WebSockets implementation used in this project is [Fleck](https://github.com/statianzo/Fleck).

The default configuration broadcasts driver positional updates every 1000ms and is therefore suited to localhost server & clients.  External clients would be better served by a server broadcasting every 5000ms or greater.  Currently there is no way to change the broadcast frequency without rebuilding the project.  This project is a proof of concept; I hope it spurs some creativity from the wider community!

Usage
=====
1. Load the iRacing sim
2. Once in the session fully (i.e. you can enter the track if you so wish), launch RivalTrackerDemo.exe.
3. RivalTrackerDemo.exe will start a WebSockets server on ws://localhost:8181.  Once connected to an iRacing client updates will be broadcast every 1000ms.  The broadcast payload is :

```javascript
{
    "trackId" : 166,
    "drivers" : {
        "3":0.274489,
        "4":0.7379593,    
        "85":0.5478597,    
        "10":0.7321385
    }  
}
```
```
trackId - the iRacing unique id of the circuit
drivers - simple object containing car numbers and current lap progress percentage (0-1)
```

A proof of concept RivalTracker client is hosted at http://www.samiad.co.uk/rivaltracker.  Visit this webpage after launching RivalTrackerDemo.exe to see the results.

Known Issues
============
* Works with iRacing replays, but not all replays will contain positional data for all drivers in a session.  If the iRacing replay doesn't contain the data, no data will be broadcast via WebSockets.
* If the application is started before iRacing is finished loading there is a chance that the SDK is not ready to send positional updates to the clients.  Should this occur you will need to close down RivalTrackerDemo.exe and relaunch once iRacing has fully loaded.
* IE8 doesn't support SVG natively.  So no go (without plugins).  IE9 doesn't support WebSockets natively.  So no go there either.  Current suggestions are for Chrome or Safari, Firefox nightly (or any Firefox > 20) until the rest catchup with the new standards.

Credits
=======
David Tucker of iRacing for providing and supporting the iRacing SDK

Scott Przybylski for his work on a C# implementation of the SDK

Jason Staten for his Fleck C# WebSockets server

License
=======
Copyright 2013 Sam Hazim

Released under GPLv3
