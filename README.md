# RhythmDoctor.Archipelago

<img align="right" src=".github/assets/logo.png" alt="RhythmDoctor.Archipelago logo">

A plugin for **Rhythm Doctor** to integrate with the [Archipelago Multi-World Randomizer](https://archipelago.gg/).

The corresponding apworld for this plugin can be found [here](https://github.com/nonperforming/Archipelago/tree/new/worlds/rhythm_doctor).
A [PopTracker](https://github.com/black-sliver/PopTracker) pack maintained by jetenergy can be found [here](https://github.com/jetenergy/rhythm-doctor-poptracker), or alternatively [Universal Tracker](https://github.com/FarisTheAncient/Archipelago) can also be used.

> [!CAUTION]
> Currently this plugin is built against the beta versions of Rhythm Doctor. You may get errors/crashes on the stable branch of the game, please see [this page](https://partner.steamgames.com/doc/store/application/branches) for instructions on how to switch to the beta branch.

> [!IMPORTANT]
> Currently, for any Rhythm Doctor version on or beyond `v1.0.5` (where the game switched to Unity 6), **BepInEx will not function properly on macOS regardless of the used architecture. No plugins will load, including the Archipelago plugin.**
> **This is not a plugin issue, and is instead a BepInEx issue.** I cannot do anything about this; if you are on macOS you will either need to wait for BepInEx to update, or boot into [some other operating system](https://asahilinux.org/) that is supported.

## Download

**There is no stable version of the plugin or apworld yet.** The latest unstable release of the plugin and apworld can be found in [Releases](https://github.com/nonperforming/RhythmDoctor.Archipelago/releases).

## Installation

**It is recommended to use BepInEx 5 over BepInEx 6 whenever possible.**

If you have BepInEx 5 installed already, skip to step 3.

1. **Install BepInEx 5**
   [Download and install BepInEx 5.](https://docs.bepinex.dev/v5.4.21/articles/user_guide/installation/index.html)
   - If you are on macOS, it is recommended to use [gib](https://github.com/toebeann/gib) to install BepInEx.
     If you are using gib, complete the setup and skip to step 3.

3. **Run the Game**
   Launch Rhythm Doctor once to ensure that BepInEx sets up the required folder structure.
   - If you're using Steam on Linux, you need to set your Steam launch options to `./run_bepinex.sh "Rhythm Doctor" # %command%`.
   - If you're using the itch.io version on Linux, you need to run the game with `./run_bepinex.sh "Rhythm Doctor"`. (Make sure you're in the Rhythm Doctor game installation directory when running this!)

4. **Install the RhythmDoctor.Archipelago plugin**
   - Obtain the plugin files from a stable release in [Releases](https://github.com/nonperforming/RhythmDoctor.Archipelago/releases/) or from [GitHub Actions](https://github.com/nonperforming/RhythmDoctor.Archipelago/actions/workflows/main.yml) (these are debug builds and may be unstable).
   - Place the following files in the `Rhythm Doctor/BepInEx/plugins` folder:
      - `RhythmDoctor.Archipelago.dll`
      - `io.github.nonperforming.pulse.dll`
      - `Assets` folder
      - `Archipelago.MultiClient.Net.dll`

5. **Configure BepInEx**
   In the `Rhythm Doctor/BepInEx/config/BepInEx.cfg` file, set `HideManagerGameObject` to `true`.
   The option should be on line 17.

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
