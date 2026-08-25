# MoanMod

[Preview video of the mod](https://tr.ee/W9rxezOVbP)

## Table of Contents

* [Version Notice](#version-notice)
* [Overview](#overview)
* [Features](#features)
* [Installation](#installation)
* [Updating / Uninstalling](#updating--uninstalling)
* [Building from Source](#building-from-source)
* [Configuration](#configuration)
* [License](#license)

## Version and OS Notices

> **Version Notice**: This mod is actively developed and tested only for the **latest version** of My Dystopian Robot Girlfriend. Using it with older game versions may result in bugs or crashes.

> [!CAUTION]
> Supported OS:<br>
> - Windows (Native)<br>
> - Linux (Not native, only via Proton **(untested)**)<br>
> - MacOS (Not native, You MAY be able to get it working on mac via [this guide](https://github.com/LAOUUUUU/MacOS-MoanMod/tree/main). **Guide is untested, and not verified, use at your own risk**)

> [!CAUTION]
> NOT supported:<br>
> - Android (I am aware that **Lemonloader** exists, but Lemonloader is outdated and does NOT work with the unity version of MDRG.)<br>
> - iOS<br>
> - Web

> This mod is only compatible with MDRG 0.97 and onwards. 0.95 and below is **NOT** supported.

## Overview

Audio and expression mod for *My Dystopian Robot Girlfriend* with dynamic moaning based on pleasure and breathing.

> [!WARNING]
> Important note: You need to have unlocked Advanced AI, moaning is disabled until then.
> Directly after unlocking Advanced AI you may notice that Jun doesn't moan as often as for example in my showcase video. Thats due to the this mod adjusting to how much she likes you, and how attracted she is to you. So she will moan more as you play the game more.

## Features

* **Pleasure-Based Responsiveness** - Moans trigger based on pleasure changes. Higher pleasure makes her more reactive, while lower pleasure requires larger changes. Sensitivity adjusts automatically throughout the scene.
* **Moan Clustering** - Moans build naturally into clusters. The first moan often leads to more, but each additional moan becomes less likely. Creates organic escalation instead of constant noise.
* **Intelligent Breathing** - Breathing becomes more frequent during intense scenes. Light activity rarely triggers breathing, but heavy moaning leads to natural breathing between sounds.
* **Dynamic Moan Frequency** - Moan speed depends on in-game stats. Higher lust and sympathy make her respond more frequently. Lower stats produce slower, more spaced-out moans.
* **Dynamic Expressions** - Sex moans adjust her facial expressions for more engaging scenes.
* **Audio Variety** - The mod prevents the same sound from repeating over and over by cycling through clips. Previous sounds need time before they can play again.
* **Moan States** - Different sounds for while-sex moans, cumming start (single startup moan), cumming ongoing, and cumming end (conclusion moan after cumming stops).
* **Configuration** - Fully configurable in-game through the [ModSettingsMenu](https://github.com/Echo5Dev/MDRG-ModSettingsMenu).

## Installation

### What you need:

* MelonLoader: [MelonLoader.Installer/releases](https://github.com/LavaGang/MelonLoader.Installer/releases/tag/4.3.0)
* Mods.zip from [IkariDevGIT/MDRGMoanMod/releases](https://github.com/IkariDevGIT/MDRGMoanMod/releases)

> Note: MAKE SURE to get the correct version, the version listed in the release (via "Compatible MDRG versions") needs to match up your MDRG Game version.

### Steps:

1. Download the game (If not already downloaded)

2. Get MelonLoader set up:

   * Download MelonLoader from the link above
   * Press "Add game manually"
   * Find and select the game's .exe file
   * Click on the game in the selection menu
   * **DO NOT enable "nightly builds"**. Install the MelonLoader version **"0.7.3"**.
   * Hit install

3. First launch:

   * Open the game once (this creates the necessary folders)

4. Install the mod:

   * Go to your game folder
   * Extract the contents of Mods.zip directly into the `/Mods/` folder
   * Make sure the files are placed correctly: `/Mods/MoanMod.dll`, `/Mods/ModSettingsMenu.dll`, and `/Mods/MoanMod/...`
   * Don't put them in a subfolder like `/Mods/Mods/`

### Expected folder structure:

```
Game Install Folder/
├── My Dystopian Robot Girlfriend.exe
├── (Other game files...)
└── Mods/
    ├── MoanMod.dll
    ├── ModSettingsMenu.dll
    └── MoanMod/
        ├── cumming/
        │   ├── start/
        │   ├── while/
        │   └── end/
        ├── while/
        └── breath/
```

> [!WARNING]
> **Important**: Make sure your game installation path **does not contain non-Latin characters** (for example Cyrillic, Chinese, Japanese, etc.).  
> Install the game in a folder with only standard English letters (e.g. `C:\Games\MDRG`). Otherwise the mod may not work.

## Updating / Uninstalling

Updating the mod works the same way as uninstalling:

1. Go to your game’s `/Mods/` folder
2. Delete `MoanMod.dll` and `/Mods/ModSettingsMenu.dll`
3. Delete the `MoanMod/` folder
4. Install the new version (From [here](https://github.com/IkariDevGIT/MDRGMoanMod/releases)) by following the [installation steps](#installation)

## Using Your Own Moans

You can replace the included audio files with your own without rebuilding the mod.

1. Close the game.
2. Go to `/Mods/MoanMod/`.
3. Add or replace `.wav` files in the folder for the sound you want to change:
- `/while/` - Regular moans during sex
- `/cumming/start/` - Played when cumming starts
- `/cumming/while/` - Played while cumming
- `/cumming/end/` - Played when cumming ends
- `/breath/` - Breathing sounds

4. If you only want your own sounds, delete the original `.wav` files from those folders.
5. Start the game again. The mod loads the audio files on startup.

You can put multiple audio files in each folder. The filenames do not matter.

> [!IMPORTANT]
> Audio files must be `.wav` files using **8-bit or 16-bit PCM** audio.

> [!WARNING]
> `/cumming/while/` must contain at least one valid `.wav` file, otherwise the mod will fail to load its audio.

## Building from Source

### Requirements
- Visual Studio 2022+ (or any C# IDE supporting .NET 6)
- .NET 6.0 SDK
- Game install with MelonLoader and Il2Cpp assemblies, see [this](#what-you-need).
- [Mod Settings Menu (MSM)](https://github.com/Echo5Dev/MDRG-ModSettingsMenu) - required for the in-game settings page
- WAV audio files (8 or 16-bit PCM format)

### Setup

1. Follow the [normal installation](#steps) up until step 4.
2. Place the ModSettingsMenu.dll in your mods folder.

3. Clone or extract the repository
4. Copy `Local.props.example` content into a new file called `Local.props`
5. Edit `Local.props` and set your game directory:
   `<GameDir>C:\Path\To\Your\Game\Install</GameDir>`
6. Open `MoanMod.sln` in Visual Studio
7. Build the project
8. The `.dll` automatically deploys to your Mods folder

## Configuration

All of the mod's tuning values (pleasure sensitivity, moan clustering, breathing rates, expression
modifiers, position multipliers, mouth animation ranges) are editable in-game through the
[Mod Settings Menu](https://github.com/Echo5Dev/MDRG-ModSettingsMenu) (Options > Mod Settings Menu >
Moan Mod). Changes apply immediately and persist across restarts. Use the "Reset to Defaults"
button to restore the original values.

The original defaults live in `MoanModDefaults.cs` if you want to change what "default" means when
building from source.

## Credits

* Sheep (The MDRG Dev) - Helped me with some parts of the code, answered many ~~stupid~~ questions i asked.
* Ivory61 - Helped with Popup code.
* im-trisha - Refactored the code.
* KlahTune - For letting me post this on the official Itch of the game.

* All the kind people from the MDRG Discord - Helped me with parts of the code, answered questions and play-tested my mod.


## License

MIT License - See LICENSE.txt
