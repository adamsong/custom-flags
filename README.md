# Custom Flags
Allows adding custom flags to Kerbal Space Program 2

## Usage
1. Install BepInEx and Spacewarp.
1. Download either the latest release from this repo, or SpaceDock
1. Unzip the downloaded file into the BepInEx plugins folder
1. Place png files into `<KSP 2 Install Directory>/flags` and they will appear in game.

## Building from source
1. Download the repo
1. Either create a solution or import the project into an existing solution
1. Drag all of the game dlls into the `external_dlls/` folder
1. Drag the SpaceWarp dll into the `external_dlls/` folder
1. Build the project
1. Copy the CustomFlags.dll file into the custom-flags folder
1. Move the custom-flags folder into the BepInEx plugins folder
