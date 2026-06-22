using System.Diagnostics;

#if DEBUG
namespace RhythmDoctor.Archipelago.Debug;

using SA.GoogleDoc;

public class DebugMenu : MonoBehaviour
{
  private enum ActivatedGUI
  {
    None,
    Main,
    Patches,
    Levels,
  }

  private ActivatedGUI _activatedGUI;

  /// <remarks>
  /// Unlike the <see cref="StoryClient"/>'s <see cref="StoryClient.ModifierManagerComponent"/>, traps from this Trap Manager will not be
  /// automatically deleted.
  /// </remarks>
  internal ArchipelagoTrapManagerClientComponent ArchipelagoTrapManagerClientComponent = new();

  private void Start()
  {
    Notify("Debug menu started");
  }

  private void Notify(string toShow)
  {
    try
    {
      LEDSign.status = toShow;
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

        if (GUI.Button(new Rect(30, 30, 150, 20), "Toggle RD Debug"))
        {
          Notify("Toggling RD Debug to " + !DebugSettings.instance.Debug);
          DebugSettings.instance.Debug = !DebugSettings.instance.Debug;
        }
        if (GUI.Button(new Rect(180, 30, 150, 20), "Dump loc"))
        {
          Notify("-\n-dumping loc-\n-");

          //foreach ((var lang, var content) in Localization.Client.SheetDictionary)
          //{
          //  Plugin.Logger.LogWarning(lang);
          //  Plugin.Logger.LogWarning(Json.Serialize(content));
          //}
        }
        if (GUI.Button(new Rect(30, 90, 300, 20), "Disable Steam Achievements"))
        {
          Notify("Applying disable achievement patch");
          Harmony.CreateAndPatchAll(typeof(DisableSteamAchievementsPatch), Plugin.PATCH_ID_DEBUG);
        }
        break;
      case ActivatedGUI.Patches:
        GUI.Box(new Rect(10, 10, 330, 890), "Rhythm Doctor Archipelago Patches Debug");
        if (GUI.Button(new Rect(30, 30, 300, 20), "Discard active traps"))
        {
          ArchipelagoTrapManagerClientComponent.ClearActiveTraps(false);
        }
        if (GUI.Button(new Rect(30, 60, 300, 20), "Discard all traps immediately"))
        {
          ArchipelagoTrapManagerClientComponent.ClearActiveTraps(false);
          ArchipelagoTrapManagerClientComponent.Traps.Clear();
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
          Plugin.ClientOld = new Client.ClientOld();
        }
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

          foreach (Region region in Enum.GetValues(typeof(Region)))
          {
            scnLevelSelect.instance.UnlockEntrance(region);
          }
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
