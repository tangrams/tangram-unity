Tangram Unity
=============


![screenshot](tangram-unity.png)

Tools for using Mapzen vector tiles in the Unity engine. The repository contains the assets and main plugin to design and generate a map region to be used in your Unity scene.

Quickstart
-----------

1. Create a [Mapzen API Key](https://mapzen.com/developers/sign_up)
2. Open the Unity plugin project
4. Drag and drop `Assets/Mapzen.prefab` in a Unity scene
4. Create a new _style_ to selectively generate and filter data for your map region: Assets > Create > Mapzen > Style
5. Start authoring the style by adding filters to select map layers and associate them with materials
6. Select the game object `Mapzen`
	- Add your API key created in 1.
	- Reference the newly created style asset in `Feature styling`
	- Give a a region name
	- Hit the `Download` button

Roadmap
-------

- Terrain support
- Tile streaming 
- Map data access from game objects

