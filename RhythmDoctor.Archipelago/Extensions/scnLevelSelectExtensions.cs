namespace RhythmDoctor.Archipelago.Extensions;

// ReSharper disable once InconsistentNaming
internal static class scnLevelSelectExtensions
{
  /// <summary>
  /// Unlock a ward's entrance.
  /// </summary>
  /// <remarks>
  /// Unlocking the <see cref="Region.RecordsRoom"/> will unlock the elevator to the records room only,
  /// not the Abandoned Ward. For this purpose, use <see cref="UnlockAbandonedWard"/>.
  /// </remarks>
  /// <param name="this"><see cref="scnLevelSelect"/> instance.</param>
  /// <param name="regionToUnlock">The region to unlock.</param>
  internal static void UnlockEntrance(this scnLevelSelect @this, Region regionToUnlock)
  {
    string[] entrances;
    switch (regionToUnlock)
    {
      case Region.SVTWard:
        entrances = [scnLevelSelect.GoToSVTWard];
        break;
      case Region.Train:
        entrances = [scnLevelSelect.GoToTrain];
        break;
      case Region.PhysiotherapyWard:
        entrances = [scnLevelSelect.GoToAthleteWard];
        break;
      case Region.RecordsRoom:
        entrances = [scnLevelSelect.MainElevator];
        break;
      case Region.Basement:
        // Also unlock Muse Dash levels.
        // Rin will normally not be visible until we complete One Shift More and complete the post-Act 3 cutscene.
        entrances = [scnLevelSelect.GoToBasement, scnLevelSelect.GoToMuseDashRoom];
        @this.ActivateEntranceRin();
        break;
      case Region.GardenRoom:
        entrances = [scnLevelSelect.GoToArtRoom];
        break;
#pragma warning disable RCS1069
      case Region.MainWard:
#pragma warning restore RCS1069
      default:
        Plugin.Logger.LogWarning(
          $"Trying to unlock {regionToUnlock} but it doesn't have an implementation/it is the Main Ward"
        );
        return;
    }

    foreach (string entrance in entrances)
    {
      Plugin.Logger.LogDebug($"Unlocking {entrance} entrance");
      @this.UnlockEntrance(@this.FindSelectableEntity(entrance));
    }
  }

  internal static void LockEntrance(this scnLevelSelect @this, SelectableEntity entranceToLock)
  {
    entranceToLock.normalEnabled = false;
    entranceToLock.hardEnabled = false;

    switch (entranceToLock.id)
    {
      case scnLevelSelect.GoToSVTWard:
        @this.mainWard.svtBlockage.SetActive(false);
        break;
      case scnLevelSelect.GoToAthleteWard:
        @this.mainWard.rooftopBlockade.SetActive(false);
        @this.nicoleAct5Blockage.gameObject.SetActive(false);
        break;
      case scnLevelSelect.MainElevator:
        @this.mainWard.SetElevatorOpened(false, 999f);
        break;
      case scnLevelSelect.GoToBasement:
        // Doesn't actually appear to do anything.
        // @this.mainWard.SetBasementDoorOpened(false, 999f);
        // Hiding the basement door object reveals a closed door underneath.
        GameObject.Find("/Scene/Corridor/RDMainWard/Seamless/Seamless_BasementDoor").SetActive(false);
        break;
    }
  }

  internal static void LockEntrance(this scnLevelSelect @this, string entranceToLock) =>
    @this.LockEntrance(@this.FindSelectableEntity(entranceToLock));

  /// <summary>
  /// Add the Finale (<see cref="scnLevelSelect.ExitVoid"/>) vertical destination to <see cref="scnLevelSelect.selectedVerticalDestinations"/>.
  /// </summary>
  /// <remarks>To be able to access the elevator, you must unlock it using <see cref="scnLevelSelect.UnlockEntrance"/>.</remarks>
  /// <param name="this"><see cref="scnLevelSelect"/> instance.</param>
  internal static void UnlockAbandonedWard(this scnLevelSelect @this)
  {
    if (@this.selectedVerticalDestinations.Any((destination) => destination.title == scnLevelSelect.ExitVoid))
    {
      Plugin.Logger.LogWarning("ExitVoid already exists in selectedVerticalDestinations");
      return;
    }

    Plugin.Logger.LogWarning("Adding ExitVoid to selectedVerticalDestinations");
    @this.selectedVerticalDestinations.Add(
      new scnLevelSelect.LevelSelectDestination(
        scnLevelSelect.ExitVoid,
        RDString.Get("levelSelect.finale"),
        RDString.Get("levelSelect.GoToVoid.day")
      )
    );
  }
}
