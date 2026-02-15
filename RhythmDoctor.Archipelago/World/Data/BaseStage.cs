namespace RhythmDoctor.Archipelago.World.Data;

internal abstract class BaseStage
{
  internal Act StageAct { get; init; }

  /// <summary>
  /// Get locations to clear for this stage.
  /// </summary>
  /// <param name="rank">Rank achieved.</param>
  /// <returns>List of location IDs to clear.</returns>
  internal abstract IEnumerable<long> GetLocationsToClear(Rank rank);
}
