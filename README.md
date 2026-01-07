# RhythmDoctor.Archipelago

<img align="right" src=".github/assets/logo.png" alt="RhythmDoctor.Archipelago logo">

A mod for **Rhythm Doctor** to integrate with the [Archipelago Multi-World Randomizer](https://archipelago.gg/), allowing you to experience Rhythm Doctor alongside other games within the Archipelago ecosystem.

## Installation

To set up the Rhythm Doctor Archipelago mod, follow these steps:

1. **Install BepInEx**
   [Download and install BepInEx 5.](https://docs.bepinex.dev/v5.4.21/articles/user_guide/installation/index.html)

2. **Run the Game**
   Launch Rhythm Doctor once to ensure that BepInEx sets up the required folder structure. If you're on Linux, you need to set your Steam launch options to `./run_bepinex.sh "Rhythm Doctor" # %command%`.

3. **Install the RhythmDoctor.Archipelago plugin**
   - Obtain the mod files from a stable release in the `Releases` section or from GitHub Actions (these are debug builds and may be unstable).
   - Place the following files in the `Rhythm Doctor/BepInEx/plugins` folder:
      - `RhythmDoctor.Archipelago.dll`
      - `io.github.nonperforming.pulse.dll`
      - `Assets` folder
      - `Archipelago.MultiClient.Net.dll`

4. Install **dependencies**
   Obtain and install [Pulse](https://github.com/nonperforming/Pulse) from [its releases page](https://github.com/nonperforming/Pulse/releases)

5. (Linux Only) **Configure BepInEx**
   In the `Rhythm Doctor/BepInEx/config/BepInEx.cfg` file, set `HideManagerGameObject` to `true`. The option should be on line 17.

## Development

### **Visual Studio** and **Rider**

1. Open the `RhythmDoctor.Archipelago.slnx`/`RhythmDoctor.Archipelago/RhythmDoctor.Archipelago.csproj` file.
2. Make your changes.
3. Build the solution.
4. Copy the following output files from `bin\<Configuration>\netstandard2.1\` to the game’s `BepInEx\plugins` directory:
    - `RhythmDoctor.Archipelago.dll`
    - `io.github.nonperforming.pulse.dll`
    - `Assets` folder
    - `Archipelago.MultiClient.Net.dll`
5. Launch the game. A successful installation will show the installed RhythmDoctor.Archipelago plugin version in the bottom left of the Main Menu.

---

Enjoy experiencing Rhythm Doctor with Archipelago’s multi-game adventure!
