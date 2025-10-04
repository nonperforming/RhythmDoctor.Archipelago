using System.Diagnostics;

#if DEBUG
namespace RhythmDoctor.Archipelago.Debug;

public class DebugMenu : MonoBehaviour
{
  private enum ActivatedGUI
  {
    None,
    Main,
    Data,
    Patches,
    Levels,
  }

  private ActivatedGUI _activatedGUI;

  private string _url = "archipelago.gg";

  // ReSharper disable once NullableWarningSuppressionIsUsed
  private string _username = null!;

  // ReSharper disable once NullableWarningSuppressionIsUsed
  private string _password = null!;
  private bool _deathLink;

  /// <remarks>
  /// Unlike the <see cref="Client"/>'s <see cref="Client.TrapManager"/>, traps from this Trap Manager will not be
  /// automatically deleted.
  /// </remarks>
  internal TrapManager TrapManager = new();

  private void Start()
  {
    Notify("Debug menu started");
  }

  private void Notify(string toShow)
  {
    try
    {
      HUD.status = toShow;
    }
    catch (NullReferenceException) { }
    Plugin.Logger.LogDebug(toShow);
  }

  private ActivatedGUI GUIButton
  {
    get
    {
      if (Input.GetKeyDown(KeyCode.F3))
        return ActivatedGUI.Main;
      else if (Input.GetKeyDown(KeyCode.F4))
        return ActivatedGUI.Data;
      else if (Input.GetKeyDown(KeyCode.F5))
        return ActivatedGUI.Patches;
      else if (Input.GetKeyDown(KeyCode.F6))
        return ActivatedGUI.Levels;
      return ActivatedGUI.None;
    }
  }

  private void Update()
  {
    ActivatedGUI gui = GUIButton;

    if (Input.GetKeyDown(KeyCode.F11) && Debugger.IsAttached)
    {
      Debugger.Break();
    }

    if (gui == ActivatedGUI.None)
      return;

    _activatedGUI = _activatedGUI == gui ? ActivatedGUI.None : gui;

    Notify($"Toggled activated debug GUI to {_activatedGUI}");
  }

  private void OnGUI()
  {
    switch (_activatedGUI)
    {
      case ActivatedGUI.None:
        break;

      case ActivatedGUI.Main:
        GUI.Box(new Rect(10, 10, 330, 180), "Rhythm Doctor Archipelago Main Debug");

        if (GUI.Button(new Rect(30, 30, 165, 20), "Toggle RD Debug"))
        {
          Notify("Toggling RD Debug to " + !DebugSettings.instance.Debug);
          DebugSettings.instance.Debug = !DebugSettings.instance.Debug;
        }
        if (GUI.Button(new Rect(185, 30, 155, 20), "Toggle autoplay"))
        {
          Notify("Toggling auto to " + !DebugSettings.instance.Auto);
          DebugSettings.instance.Debug = !DebugSettings.instance.Auto;
        }

        GUI.Label(new Rect(30, 50, 150, 20), "URL");
        _url = GUI.TextField(new Rect(180, 50, 150, 20), _url);

        GUI.Label(new Rect(30, 70, 150, 20), "Username");
        _username = GUI.TextField(new Rect(180, 70, 150, 20), _username);

        GUI.Label(new Rect(30, 90, 150, 20), "Password");
        _password = GUI.TextField(new Rect(180, 90, 150, 20), _password);

        GUI.Label(new Rect(30, 110, 150, 20), "Death Link");
        _deathLink = GUI.Toggle(new Rect(180, 110, 150, 20), _deathLink, "");

        if (GUI.Button(new Rect(30, 130, 300, 20), "Connect"))
        {
          Plugin.Client = new Client.Client(_url, _username, _password);
        }
        break;
      case ActivatedGUI.Data:
        GUI.Box(new Rect(10, 10, 330, 150), "Rhythm Doctor Archipelago Data Debug");
        break;
      case ActivatedGUI.Patches:
        GUI.Box(new Rect(10, 10, 330, 890), "Rhythm Doctor Archipelago Patches Debug");
        if (GUI.Button(new Rect(30, 30, 300, 20), "Discard active traps"))
        {
          TrapManager.ClearActiveTraps(false);
        }
        if (GUI.Button(new Rect(30, 60, 300, 20), "Discard all traps immediately"))
        {
          TrapManager.ClearActiveTraps(false);
          TrapManager.Traps.Clear();
        }
        if (GUI.Button(new Rect(30, 90, 300, 20), "Add Chilli Speed Trap"))
        {
          Notify("Adding ChilliSpeedTrap patch");
          TrapManager.AddTrap(new ChilliSpeedTrapPatch());
        }
        if (GUI.Button(new Rect(30, 120, 300, 20), "Add Ice Speed Trap"))
        {
          Notify("Adding IceSpeedTrap patch");
          TrapManager.AddTrap(new IceSpeedTrapPatch());
        }

        if (GUI.Button(new Rect(30, 150, 300, 20), "Add Strong Heart Powerup"))
        {
          Notify("Adding StrongHeartPowerup patch");
          TrapManager.AddTrap(new StrongHeartPowerupPatch());
        }
        if (GUI.Button(new Rect(30, 180, 300, 20), "Add Fragile Heart Powerup"))
        {
          Notify("Adding FragileSpeedTrap patch");
          TrapManager.AddTrap(new FragileHeartTrapPatch());
        }
        if (GUI.Button(new Rect(30, 210, 300, 20), "Add Easy Mode Powerup"))
        {
          Notify("Adding EasyModePowerup patch");
          TrapManager.AddTrap(new EasyDifficultyPowerupPatch());
        }
        if (GUI.Button(new Rect(30, 240, 300, 20), "Add Hard Mode Trap"))
        {
          Plugin.Logger.LogInfo("Adding HardModeTrap patch");
          TrapManager.AddTrap(new HardDifficultyTrapPatch());
        }
        if (GUI.Button(new Rect(30, 270, 300, 20), "Add Ghost Tap Trap"))
        {
          Notify("Adding GhostTapTrap patch");
          TrapManager.AddTrap(new GhostTapTrapPatch());
        }
        if (GUI.Button(new Rect(30, 300, 300, 20), "Add Scramble Characters Trap"))
        {
          Notify("Adding ScrambleCharactersTrap patch");
          TrapManager.AddTrap(new ScrambleCharactersTrapPatch());
        }
        if (GUI.Button(new Rect(30, 330, 300, 20), "Add Scramble Beatsounds Trap"))
        {
          Notify("Adding ScrambleBeatsoundsTrap patch");
          TrapManager.AddTrap(new ScrambleBeatsoundsTrapPatch());
        }
        if (GUI.Button(new Rect(30, 360, 300, 20), "Add Scramble Hitsounds Trap"))
        {
          Notify("Adding ScrambleHitsoundsTrap patch");
          TrapManager.AddTrap(new ScrambleHitsoundsTrapPatch());
        }
        if (GUI.Button(new Rect(30, 390, 300, 20), "Recreate TrapManager"))
        {
          Notify("Recreating TrapManager");
          TrapManager.ClearActiveTraps(false);
          TrapManager = new TrapManager();
        }

        if (GUI.Button(new Rect(30, 450, 300, 20), "Apply post-login patches"))
        {
          Notify("Applying post-login patches");
          Plugin.ApplyGameplayPatches();
        }
        if (GUI.Button(new Rect(30, 480, 300, 20), "Unapply post-login patches"))
        {
          Notify("Unapplying post-login patches");
          Plugin.UnapplyGameplayPatches();
        }

        if (GUI.Button(new Rect(30, 510, 300, 20), "Unapply debug patches"))
        {
          Notify("Unapplying debug patches");
          Plugin.UnapplyDebugPatches();
        }

        if (GUI.Button(new Rect(30, 540, 300, 20), "Create Client class"))
        {
          Notify("Creating empty Client");
          Plugin.Client = new Client.Client();
        }

        if (GUI.Button(new Rect(30, 570, 300, 20), "Push all traps to Active"))
        {
          Notify("Pushing all traps to Active");
          TrapManager.ApplyApplicableTraps(Level.None);
          TrapManager.PromotePreviewTrapsToActiveTraps();
        }

        string traps = "";
        foreach (ITrap trap in TrapManager.Traps)
        {
          traps += $"{trap.Name} ";
        }

        string trapsPreview = "";
        foreach ((_, ITrap trap) in TrapManager._previewTraps)
        {
          trapsPreview += $"{trap.Name} ";
        }

        string trapsActive = "";
        foreach ((_, ITrap trap) in TrapManager._activeTraps)
        {
          trapsActive += $"{trap.Name} ";
        }

        GUI.Label(new Rect(30, 630, 300, 300), $"Traps: {traps}\nPreview: {trapsPreview}\nActive: {trapsActive}");

        break;
      case ActivatedGUI.Levels:
        GUI.Box(new Rect(10, 10, 330, 300), "Rhythm Doctor Archipelago Levels Debug");

        if (GUI.Button(new Rect(30, 30, 300, 20), "Lock all"))
        {
          Notify("Locking all levels");
          foreach (Level level in Enum.GetValues(typeof(Level)))
          {
            Persistence.SetLevelRank(level, Rank.NotAvailable, true);
          }
          scnBase.GoToScene("scnLevelSelect");
        }

        if (GUI.Button(new Rect(30, 60, 300, 20), "Unlock all"))
        {
          Notify("Unlocking all levels");
          foreach (Level level in Enum.GetValues(typeof(Level)))
          {
            Persistence.SetLevelRank(level, Rank.NotFinished, true);
          }
          scnBase.GoToLevelSelect();
        }

        if (GUI.Button(new Rect(30, 90, 300, 20), "Unlock all entrances"))
        {
          Notify("Unlocking all entrances");

          scnLevelSelect.instance.UnlockEntrance(scnLevelSelect.instance.FindSelectableEntity("GoToSVTWard"));
          scnLevelSelect.instance.UnlockEntrance(scnLevelSelect.instance.FindSelectableEntity("GoToTrain"));
          scnLevelSelect.instance.UnlockEntrance(scnLevelSelect.instance.FindSelectableEntity("GoToBasement"));
          scnLevelSelect.instance.UnlockEntrance(scnLevelSelect.instance.FindSelectableEntity("GoToMuseDashRoom"));
          scnLevelSelect.instance.UnlockEntrance(scnLevelSelect.instance.FindSelectableEntity("GoToAthleteWard"));
          scnLevelSelect.instance.UnlockEntrance(scnLevelSelect.instance.FindSelectableEntity("GoToArtRoom"));

          scnLevelSelect.instance.ActivateEntranceRin();
        }

        if (GUI.Button(new Rect(30, 120, 300, 20), "Unlock level 3-1"))
        {
          Notify("Unlocking 3-1");
          Persistence.SetLevelRank(Level.Garden, Rank.NotFinished);
          scnBase.GoToLevelSelect();
        }

        if (GUI.Button(new Rect(30, 150, 300, 20), "Unlock level 3-2N"))
        {
          Notify("Unlocking 3-2N");
          Persistence.SetLevelRank(Level.Lounge, Rank.NotFinished);
          scnBase.GoToLevelSelect();
        }

        if (GUI.Button(new Rect(30, 180, 300, 20), "S+ selected level"))
        {
          if (scnLevelSelect.instance.selectedEntity is not SelectableCharacter selectableCharacter)
          {
            return;
          }

          Level selectedLevel = selectableCharacter.levels[scnLevelSelect.instance.currentDifficulty];
          Notify($"Setting rank of {selectedLevel} to S+");
          Persistence.SetLevelRank(selectedLevel, Rank.Splus, true);
          Persistence.SetLastPlayedLevel(selectedLevel);
          scnBase.GoToLevelSelect();
        }

        if (GUI.Button(new Rect(30, 210, 300, 20), "Perfect all levels"))
        {
          Notify("Setting rank of all levels to S+");
          foreach (Level level in Enum.GetValues(typeof(Level)))
          {
            Persistence.SetLevelRank(level, Rank.Splus, true);
          }
          scnBase.GoToLevelSelect();
        }

        if (GUI.Button(new Rect(30, 240, 300, 20), "Dump level data"))
        {
          Notify("Dumping level data - check console");
          LogData.Level(RDLevelData.current);
        }

        if (GUI.Button(new Rect(30, 270, 300, 20), "Lock selected level"))
        {
          if (scnLevelSelect.instance.selectedEntity is not SelectableCharacter selectableCharacter)
          {
            return;
          }

          Level selectedLevel = selectableCharacter.levels[scnLevelSelect.instance.currentDifficulty];
          Notify($"Setting rank of {selectedLevel} to NotAvailable");
          Persistence.SetLevelRank(selectedLevel, Rank.NotAvailable, true);

          Persistence.SetLastPlayedLevel(selectedLevel);
          scnBase.GoToLevelSelect();
        }

        break;

      default:
        throw new ArgumentOutOfRangeException();
    }
  }
}
#endif
