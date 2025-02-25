# RhythmDoctor.Archipelago

<img align="right" src=".github/assets/logo.png" alt="RhythmDoctor.Archipelago logo">

A mod for **Rhythm Doctor** to integrate with the [Archipelago Multi-World Randomizer](https://archipelago.gg/), allowing you to experience Rhythm Doctor alongside other games within the Archipelago ecosystem.

## Installation

To set up the Rhythm Doctor Archipelago mod, follow these steps:

1. **Download BepInEx**
   Download [BepInEx 5.x.x](https://github.com/BepInEx/BepInEx/releases/) (make sure not to download any version newer than 5.x.x).

2. **Extract BepInEx**
   Extract the contents of the downloaded BepInEx package directly into your Rhythm Doctor installation directory.

3. **Run the Game**
   Launch Rhythm Doctor once to ensure that BepInEx sets up the required folder structure.

4. **Install the Mod Files**
   - Obtain the mod files from GitHub Actions (these are debug builds and may be unstable) or from a stable release in the `Releases` section.
   - Place the following files in the `Rhythm Doctor\BepInEx\plugins` folder:
      - `RhythmDoctor.Archipelago.dll`
      - `World` folder
      - `Archipelago.MultiClient.Net.dll`
      - `YamlDotNet.dll`

## Development

### **Visual Studio** and **Rider**

1. Open the `.sln` file.
2. Make your changes.
3. Build the solution.
4. Copy the following output files from `bin\Debug\netstandard2.1\` to the game’s `BepInEx\plugins` directory:
    - `RhythmDoctor.Archipelago.dll`
    - `World` folder
    - `Archipelago.MultiClient.Net.dll`
    - `YamlDotNet.dll`
5. Launch the game. A successful installation will show the current mod version in the bottom left of the Main Menu.

---

Enjoy experiencing Rhythm Doctor with Archipelago’s multi-game adventure!
