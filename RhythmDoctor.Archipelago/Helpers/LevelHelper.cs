namespace RhythmDoctor.Archipelago.Helpers;

internal static class LevelHelper
{
  /// <summary>
  /// Unlock a ward's entrance.
  /// </summary>
  /// <param name="regionToUnlock">The ward to unlock</param>
  internal static void UnlockEntrance(Region regionToUnlock)
  {
    string name;
    switch (regionToUnlock)
    {
      case Region.SVTWard:
        name = "GoToSVTWard";
        break;
      case Region.Train:
        name = "GoToTrain";
        break;
      case Region.PhysiotherapyWard:
        name = "GoToAthleteWard";
        break;
      case Region.Basement:
        name = "GoToBasement";
        break;
      case Region.ArtRoom:
        name = "GoToArtRoom";
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

    Plugin.Logger.LogDebug($"Unlocking {name} entrance");
    scnLevelSelect.instance.UnlockEntrance(scnLevelSelect.instance.FindSelectableEntity(name));
  }
}
