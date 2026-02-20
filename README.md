# MoanMod

> **Version Notice**: This mod is actively developed and tested only for the **latest version** of My Dystopian Robot Girlfriend. Using it with older game versions may result in bugs or crashes.

Audio and expression mod for *My Dystopian Robot Girlfriend* with pleasure-based moaning and breathing.

## Features

- **Pleasure-Based Responsiveness** - Moans trigger based on pleasure changes. Higher pleasure makes her more reactive, while lower pleasure requires larger changes. Sensitivity adjusts automatically throughout the scene.
- **Sex Scene Clustering** - Moans build naturally into clusters. The first moan often leads to more, but each additional moan becomes less likely. Creates organic escalation instead of constant noise.
- **Intelligent Breathing** - Breathing becomes more frequent during intense scenes. Light activity rarely triggers breathing, but heavy moaning leads to natural breathing between sounds.
- **Dynamic Moan Frequency** - Moan speed depends on in-game stats. Higher lust and sympathy make her respond more frequently. Lower stats produce slower, more spaced-out moans.
- **Expression Control** - Sex moans adjust her facial expressions for more engaging scenes. Expressions reset when scenes end.
- **Audio Variety** - The mod prevents the same sound from repeating over and over by cycling through clips. Previous sounds need time before they can play again.
- **Scene States** - Different sounds for scene start (single startup moan), ongoing action, and scene end (conclusion moan after cumming stops).

## Installation

### What you need:
- MelonLoader: https://github.com/LavaGang/MelonLoader/releases
- Mods.zip from https://github.com/IkariDevGIT/MDRGMoanMod/releases

### Steps:

1. Get MelonLoader set up:
   - Download MelonLoader from the link above
   - Press "Add game manually"
   - Find and select the game's .exe file
   - Click on the game in the selection menu
   - Enable "nightly builds"
   - Hit install

2. First launch:
   - Open the game once (this creates the necessary folders)

3. Install the mod:
   - Go to your game folder
   - Extract the contents of Mods.zip directly into the `/Mods/` folder
   - Make sure the files are placed correctly: `/Mods/MoanMod.dll` and `/Mods/MoanMod/...`
   - Don't put them in a subfolder like `/Mods/Mods/`

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

## Building from Source

### Requirements
- Visual Studio 2022+ (or any C# IDE supporting .NET 6)
- .NET 6.0 SDK
- Game install with MelonLoader and Il2Cpp assemblies

### Setup

1. Clone or extract the repository
2. Edit `MoanMod.csproj` and set your game directory:
   ```xml
   <GameDir>C:\Path\To\Your\Game\Install</GameDir>
   ```
3. Open `MoanMod.sln` in Visual Studio
4. Build the project
5. The `.dll` automatically deploys to your Mods folder

## Configuration

Edit `MoanModConfig.cs` to adjust:
- Threshold sensitivity and pleasure scaling
- Cluster behavior (max moans, probabilities, repeat chances)
- Breathing trigger rates based on moan frequency
- Expression modifiers (lewdness and happiness)
- Position-specific multipliers
- Mouth animation ranges

Rebuild after making changes.

## Requirements
- My Dystopian Robot Girlfriend (latest version)
- MelonLoader
- WAV audio files (8 or 16-bit PCM format)

## License
MIT License - See LICENSE.txt