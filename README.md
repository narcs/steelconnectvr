# steelconnectvr
A Virtual Reality (VR) app to use <a href="https://www.riverbed.com/au/products/steelconnect.html">SteelConnect</a> services.

## Requirements
1. Unity 2018 or higher recommended
2. `Assets` directory with external assets

## Setup 
1. Download `Assets` directory with external packages. Copy and paste over existing `Assets` directory in project root directory.
2. Create <a href="https://www.mapbox.com/">Mapbox</a> account and note access token.
3. Open project in Unity. Enter access token and verify when Mapbox Setup window appears. Mapbox Setup can be found at `Mapbox > Setup` menu.
4. Enter SteelConnect credentials at `Assets > Scripts > Lib > SteelConnect.cs` under function `SteelConnect()`.

## Main Scene
Main Unity scene can be found at `Assets > Scenes > SteelConnect.unity`.

## Google Daydream controls
The app uses <a href="https://support.google.com/daydream/answer/7184597?hl=en">Daydream</a> touchpad and app button for interactions.
### General interactions
#### Touchpad
Touchpad click used for:
* Pressing buttons
* Click and hold on WANs to create uplinks
* Double click globe to transition to flat map centered on that position

Swipe used for:
* Log window history scrolling
* Flat map interactions

#### App button
Used for:
  * Cancelling out of 'Create' mode
  * Cancelling out of 'Delete' mode

### Flat map interactions
Touchpad:
* Point at map and swipe to pan
* Point at map and click to zoom in

App button:
* Point at map and press to zoom out

## Unity Controls
Google Daydream controller (<a href="https://developers.google.com/vr/discover/controllers">https://developers.google.com/vr/discover/controllers</a>) can be simulated with keyboard and mouse.
* Hold <kbd>Shift</kbd> to activate pointer. Use mouse to move pointer.
  * While pointer is active, hold <kbd>Ctrl</kbd> also and move mouse to emulate swipe on Daydream touchpad.
* Hold <kbd>Alt</kbd> and use mouse to rotate camera yaw and pitch.
* Hold <kbd>Ctrl</kbd> and use mouse to rotate camera roll.
* Left-click on mouse simulates Daydream controller touch pad click.
* Right-click on mouse simulates Daydream controller App button press.

## Pointer modes
* Normal mode - white
  * Default mode
* Create mode - green
  * Create sites
* Delete mode - red
  * Delete sites

## Features
Features of VR app are:
* Create and delete sites
* Create and delete uplinks
* Show information for:
  * Sites
  * Sitelinks
  * WANs
  * Uplinks
* Change view from globe to flat and vice versa.
  * Double touchpad click on globe to zoom in on flat map.
* Log window. Point and swipe to scroll through history.

### Entity information
Move pointer and hover over entities to display more information.

### Creating Sites
The VR keyboard only appears when running on Android. When running on PC, use keyboard to enter fields.

## Changing Mapbox map styles
* To change globe map style, select `EarthSphere > GlobeMap` object in Unity scene.
* To change flat map style,  select `PlayerPivot > Map` object in Unity scene.
1. Grab Mapbox style URL from <a href="https://www.mapbox.com/studio/">account</a>.
2. Paste style URL in `Abstract Map (Script) > IMAGE > Map Id / Style URL` field.

## Known Issues
### Flat Map
* Panning may cause sites to be out of sync with the map
  * This seems to occur when you try to pan in different directions too quickly in succession. 
* Zooming is erratic, may change the map size
  *This is due to how mapbox handles zooming, and tiles
* Position clicked to centre zoom on may not be the exact centre.
  *Due to mapbox tiles/zooming methods and geocoding possibly being slightly off.
* Create Site doesn't really work with flat map - keyboard clips through map and other objects

## To Do
* Make WAN `Panel` object `Rect Transform` Width size dynamic and based on number of WANs.