# RhythmDoctor.Archipelago

<img align="right" src=".github/assets/logo.png" alt="RhythmDoctor.Archipelago logo">

A plugin for **Rhythm Doctor** to integrate with the [Archipelago Multi-World Randomizer](https://archipelago.gg/).

The corresponding apworld for this plugin can be found [here](https://github.com/nonperforming/Archipelago/tree/new/worlds/rhythm_doctor).

> [!IMPORTANT]
> Currently this plugin is built against the beta versions of Rhythm Doctor. You may get errors/crashes on the stable branch of the game, please see [this page](https://partner.steamgames.com/doc/store/application/branches) for instructions on how to switch to the beta branch.

## Download

**There is no stable version of the plugin or apworld yet.** The latest unstable version of the plugin can be downloaded [here](https://nightly.link/nonperforming/RhythmDoctor.Archipelago/workflows/main/main/RhythmDoctor.Archipelago.zip), and the built apworld can be found in the [Archipelago Discord](https://discord.gg/8Z65BR2) under the Rhythm Doctor thread in the future-game-design forum.

## Installation
If you have BepInEx 5 installed already, skip to step 3.

1. **Install BepInEx 5**
   [Download and install BepInEx 5.](https://docs.bepinex.dev/v5.4.21/articles/user_guide/installation/index.html)

2. **Run the Game**
   Launch Rhythm Doctor once to ensure that BepInEx sets up the required folder structure.
   - If you're using Steam on Linux, you need to set your Steam launch options to `./run_bepinex.sh "Rhythm Doctor" # %command%`.
   - If you're using the itch.io version on Linux, you need to run the game with `./run_bepinex.sh "Rhythm Doctor"`. (Make sure you're in the Rhythm Doctor game installation directory when running this!)

3. **Install the RhythmDoctor.Archipelago plugin**
   - Obtain the plugin files from a stable release in the `Releases` section or from GitHub Actions (these are debug builds and may be unstable).
   - Place the following files in the `Rhythm Doctor/BepInEx/plugins` folder:
      - `RhythmDoctor.Archipelago.dll`
      - `io.github.nonperforming.pulse.dll`
      - `Assets` folder
      - `Archipelago.MultiClient.Net.dll`

4. **Configure BepInEx**
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
