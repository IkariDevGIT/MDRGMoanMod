# MoanMod

> # ⚠️ IMPORTANT WARNING ⚠️
>
> **DO NOT INSTALL MELONLOADER OR THIS MOD YET!**
>
> Due to **Cpp2IL incompatibilities** (a dependency of MelonLoader) with the **Unity version this game uses**, **installing MelonLoader will currently make the game crash immediately**.
>
> You **must wait** from **21st February 2025** until **~23rd February 2025**.
>
> After **23rd February**, check this repository again:
>
> > **If this warning is gone**, you may follow the guide below and install the mod.
> > **If this warning is still here**, **DO NOT** attempt installation, it will crash. Wait for the required update.
>
> **Ignoring this warning will break the game. Seriously, do not skip it.**

## Table of Contents

* [Version Notice](#version-notice)
* [Overview](#overview)
* [Features](#features)
* [Installation](#installation)
* [Updating / Uninstalling](#updating--uninstalling)
* [Building from Source](#building-from-source)
* [Configuration](#configuration)
* [Requirements](#requirements)
* [License](#license)

## Version Notice

> **Version Notice**: This mod is actively developed and tested only for the **latest version** of My Dystopian Robot Girlfriend. Using it with older game versions may result in bugs or crashes.

> **Lemonloader Notice**: Lemonloader is **not** supported.

> This mod is only compatible with MDRG 0.95 and onwards.

## Overview

Audio and expression mod for *My Dystopian Robot Girlfriend* with dynamic moaning based on pleasure and breathing.

## Features

* **Pleasure-Based Responsiveness** - Moans trigger based on pleasure changes. Higher pleasure makes her more reactive, while lower pleasure requires larger changes. Sensitivity adjusts automatically throughout the scene.
* **Moan Clustering** - Moans build naturally into clusters. The first moan often leads to more, but each additional moan becomes less likely. Creates organic escalation instead of constant noise.
* **Intelligent Breathing** - Breathing becomes more frequent during intense scenes. Light activity rarely triggers breathing, but heavy moaning leads to natural breathing between sounds.
* **Dynamic Moan Frequency** - Moan speed depends on in-game stats. Higher lust and sympathy make her respond more frequently. Lower stats produce slower, more spaced-out moans.
* **Dynamic Expressions** - Sex moans adjust her facial expressions for more engaging scenes.
* **Audio Variety** - The mod prevents the same sound from repeating over and over by cycling through clips. Previous sounds need time before they can play again.
* **Moan States** - Different sounds for while-sex moans, cumming start (single startup moan), cumming ongoing, and cumming end (conclusion moan after cumming stops).

## Installation

### What you need:

* MelonLoader: [LavaGang/MelonLoader/releases](https://github.com/LavaGang/MelonLoader/releases)
* Mods.zip from [IkariDevGIT/MDRGMoanMod/releases](https://github.com/IkariDevGIT/MDRGMoanMod/releases)

> Note: MAKE SURE to get the correct version, the version listed in the release (via "Compatible MDRG versions") needs to match up your MDRG Game version.

### Steps:

1. Download the game (If not already downloaded)

2. Get MelonLoader set up:

   * Download MelonLoader from the link above
   * Press "Add game manually"
   * Find and select the game's .exe file
   * Click on the game in the selection menu
   * Enable "nightly builds"
   * Hit install

3. First launch:

   * Open the game once (this creates the necessary folders)

4. Install the mod:

   * Go to your game folder
   * Extract the contents of Mods.zip directly into the `/Mods/` folder
   * Make sure the files are placed correctly: `/Mods/MoanMod.dll` and `/Mods/MoanMod/...`
   * Don't put them in a subfolder like `/Mods/Mods/`

### Expected folder structure:

```
Game Install Folder/
├── My Dystopian Robot Girlfriend.exe
├── (Other game files...)
└── Mods/
    ├── MoanMod.dll
    └── MoanMod/
        ├── cumming/
        │   ├── start/
        │   ├── while/
        │   └── end/
        ├── while/
        └── breath/
```

## Updating / Uninstalling

Updating the mod works the same way as uninstalling:

1. Go to your game’s `/Mods/` folder
2. Delete `MoanMod.dll`
3. Delete the `MoanMod/` folder
4. Install the new version (From [here](https://github.com/IkariDevGIT/MDRGMoanMod/releases)) by following the [installation steps](#installation)

## Building from Source

### Requirements
- Visual Studio 2022+ (or any C# IDE supporting .NET 6)
- .NET 6.0 SDK
- Game install with MelonLoader and Il2Cpp assemblies

### Setup

1. Download the game (If not already downloaded)

2. Get MelonLoader set up:
   - Download MelonLoader from the link above
   - Press "Add game manually"
   - Find and select the game's .exe file
   - Click on the game in the selection menu
   - Enable "nightly builds"
   - Hit install

3. First launch:
   - Open the game once (this creates the necessary folders)

4. Clone or extract the repository
5. Edit `MoanMod.csproj` and set your game directory:
   `<GameDir>C:\Path\To\Your\Game\Install</GameDir>`
6. Open `MoanMod.sln` in Visual Studio
7. Build the project
8. The `.dll` automatically deploys to your Mods folder

## Configuration

Edit `MoanModConfig.cs` to adjust:

* Threshold sensitivity and pleasure scaling
* Cluster behavior (max moans, probabilities, repeat chances)
* Breathing trigger rates based on moan frequency
* Expression modifiers (lewdness and happiness)
* Position-specific multipliers
* Mouth animation ranges

Rebuild after making changes.

## Credits

* Sheep (The MDRG Dev) - Helped me with some parts of the code, answered many ~~stupid~~ questions i asked.
* Ivory61 - Helped with Popup code.

* All the kind people from the MDRG Discord - Helped me with parts of the code, answered questions and play-tested my mod.


## Requirements

* My Dystopian Robot Girlfriend (latest version)
* MelonLoader
* WAV audio files (8 or 16-bit PCM format)

## License

MIT License - See LICENSE.txt
