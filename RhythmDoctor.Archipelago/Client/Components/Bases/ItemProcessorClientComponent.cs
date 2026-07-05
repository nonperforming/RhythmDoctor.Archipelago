namespace RhythmDoctor.Archipelago.Client.Components.Interfaces;

internal abstract class ItemProcessorClientComponent : ClientComponent
{
  /// <returns>True if item was processed without issue</returns>
  internal abstract bool HandleItemInitial(ItemInfo itemInfo);
  
  /// <returns>True if item was processed without issue</returns>
  internal abstract bool HandleItem(ItemInfo itemInfo);
}
