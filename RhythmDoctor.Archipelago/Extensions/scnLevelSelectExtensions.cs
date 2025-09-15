namespace RhythmDoctor.Archipelago.Helpers;

// ReSharper disable once InconsistentNaming
internal static class scnLevelSelectExtensions
{
  /// <summary>
  /// Unlock a ward's entrance.
  /// </summary>
  /// <param name="this">scnLevelSelect instance</param>
  /// <param name="regionToUnlock">The ward to unlock</param>
  internal static void UnlockEntrance(this scnLevelSelect @this, Region regionToUnlock)
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
    @this.UnlockEntrance(@this.FindSelectableEntity(name));

    if (regionToUnlock == Region.Basement)
    {
      // Unlock Muse Dash levels.
      // Rin will normally not be visible until we complete One Shift More and complete the post-Act 3 cutscene.
      Plugin.Logger.LogDebug("Unlocking Muse Dash levels");
      @this.UnlockEntrance(@this.FindSelectableEntity("GoToMuseDashRoom"));
      @this.ActivateEntranceRin();
    }
  }
}
