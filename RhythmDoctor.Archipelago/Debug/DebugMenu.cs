using System.Diagnostics;
using RDLevelEditor;

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

  private ActivatedGUI activatedGUI;

  private string _url = "archipelago.gg";

  // ReSharper disable once NullableWarningSuppressionIsUsed
  private string _username = null!;

  // ReSharper disable once NullableWarningSuppressionIsUsed
  private string _password = null!;
  private bool _deathLink;

  /// <remarks>
  /// Unlike the <see cref="Client"/>'s <see cref="Client.trapManager"/>, traps from this Trap Manager will not be
  /// automatically deleted.
  /// </remarks>
  internal TrapManager trapManager = new();

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

    activatedGUI = activatedGUI == gui ? ActivatedGUI.None : gui;

    Notify($"Toggled activated debug GUI to {activatedGUI}");
  }

  private void OnGUI()
  {
    switch (activatedGUI)
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
        GUI.Box(new Rect(10, 10, 330, 600), "Rhythm Doctor Archipelago Patches Debug");
        if (GUI.Button(new Rect(30, 30, 300, 20), "Discard active traps"))
        {
          trapManager.ClearActiveTraps(false);
        }
        if (GUI.Button(new Rect(30, 60, 300, 20), "Discard all traps immediately"))
        {
          trapManager.ClearActiveTraps(false);
          trapManager.Traps.Clear();
        }
        if (GUI.Button(new Rect(30, 90, 300, 20), "Add Chilli Speed Trap"))
        {
          Notify("Adding ChilliSpeedTrap patch");
          trapManager.AddTrap(new ChilliSpeedTrapPatch());
        }
        if (GUI.Button(new Rect(30, 120, 300, 20), "Add Ice Speed Trap"))
        {
          Notify("Adding IceSpeedTrap patch");
          trapManager.AddTrap(new IceSpeedTrapPatch());
        }

        if (GUI.Button(new Rect(30, 150, 300, 20), "Add Strong Heart Powerup"))
        {
          Notify("Adding StrongHeartPowerup patch");
          trapManager.AddTrap(new StrongHeartPowerupPatch());
        }
        if (GUI.Button(new Rect(30, 180, 300, 20), "Add Fragile Heart Powerup"))
        {
          Notify("Adding FragileSpeedTrap patch");
          trapManager.AddTrap(new FragileHeartTrapPatch());
        }
        if (GUI.Button(new Rect(30, 210, 300, 20), "Add Easy Mode Powerup"))
        {
          Notify("Adding EasyModePowerup patch");
          trapManager.AddTrap(new EasyModePowerupPatch());
        }
        if (GUI.Button(new Rect(30, 240, 300, 20), "Add Hard Mode Trap"))
        {
          Plugin.Logger.LogInfo("Adding HardModeTrap patch");
          trapManager.AddTrap(new HardModeTrapPatch());
        }
        if (GUI.Button(new Rect(30, 270, 300, 20), "Add Ghost Tap Trap"))
        {
          Notify("Adding GhostTapTrap patch");
          trapManager.AddTrap(new GhostTapTrapPatch());
        }
        if (GUI.Button(new Rect(30, 300, 300, 20), "Recreate TrapManager"))
        {
          Notify("Recreating TrapManager");
          trapManager.ClearActiveTraps(false);
          trapManager = new();
        }

        if (GUI.Button(new Rect(30, 360, 300, 20), "Apply post-login patches"))
        {
          Notify("Applying post-login patches");
          Plugin.ApplyGameplayPatches();
        }
        if (GUI.Button(new Rect(30, 390, 300, 20), "Unapply post-login patches"))
        {
          Notify("Unapplying post-login patches");
          Plugin.UnapplyGameplayPatches();
        }

        if (GUI.Button(new Rect(30, 420, 300, 20), "Create Client class"))
        {
          Notify("Creating empty Client");
          Plugin.Client = new();
        }

        string traps = "";
        foreach (ITrap trap in trapManager.Traps)
        {
          traps += $"{trap.Name} ";
        }

        string trapsPreview = "";
        foreach ((_, ITrap trap) in trapManager._previewTraps)
        {
          trapsPreview += $"{trap.Name} ";
        }

        string trapsActive = "";
        foreach ((_, ITrap trap) in trapManager._activeTraps)
        {
          trapsActive += $"{trap.Name} ";
        }

        GUI.Label(new Rect(30, 450, 300, 100), $"Traps: {traps}\nPreview: {trapsPreview}\nActive: {trapsActive}");

        break;
      case ActivatedGUI.Levels:
        GUI.Box(new Rect(10, 10, 330, 270), "Rhythm Doctor Archipelago Levels Debug");

        if (GUI.Button(new Rect(30, 30, 300, 20), "Lock all"))
        {
          Notify("Locking all levels");
          foreach (Level level in Enum.GetValues(typeof(Level)))
          {
            Persistence.SetLevelRank(level, Rank.NotAvailable, force: true);
          }
          scnBase.GoToScene("scnLevelSelect");
        }

        if (GUI.Button(new Rect(30, 60, 300, 20), "Unlock all"))
        {
          Notify("Unlocking all levels");
          foreach (Level level in Enum.GetValues(typeof(Level)))
          {
            Persistence.SetLevelRank(level, Rank.NotFinished, force: true);
          }
          scnBase.GoToLevelSelect();
        }

        if (GUI.Button(new Rect(30, 90, 300, 20), "Unlock all entrances"))
        {
          Notify("Unlocking all entrances");
          scnLevelSelect.instance.UnlockEntrance(scnLevelSelect.instance.FindSelectableEntity("GoToSVTWard"));
          scnLevelSelect.instance.UnlockEntrance(scnLevelSelect.instance.FindSelectableEntity("GoToTrain"));
          scnLevelSelect.instance.UnlockEntrance(scnLevelSelect.instance.FindSelectableEntity("GoToBasement"));
          scnLevelSelect.instance.UnlockEntrance(scnLevelSelect.instance.FindSelectableEntity("GoToAthleteWard"));
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
            return;

          Level selectedLevel = selectableCharacter.levels[scnLevelSelect.instance.currentDifficulty];
          Notify($"Setting rank of {selectedLevel} to S+");
          Persistence.SetLevelRank(selectedLevel, Rank.Splus, force: true);
          Persistence.SetLastPlayedLevel(selectedLevel);
          scnBase.GoToLevelSelect();
        }

        if (GUI.Button(new Rect(30, 210, 300, 20), "Perfect all levels"))
        {
          Notify("Setting rank of all levels to S+");
          foreach (Level level in Enum.GetValues(typeof(Level)))
          {
            Persistence.SetLevelRank(level, Rank.Splus, force: true);
          }
          scnBase.GoToLevelSelect();
        }

        if (GUI.Button(new Rect(30, 240, 300, 20), "Dump level data"))
        {
          Notify("Dumping level data - check console");
          LogData.Level(RDLevelData.current);
        }

        break;

      default:
        throw new ArgumentOutOfRangeException();
    }
  }
}
#endif
