namespace RhythmDoctor.Archipelago.Client.Components.Interfaces;

internal interface IItemProcessorClientComponent : IClientComponent
{
  /// <returns>True if item was processed without issue</returns>
  internal bool HandleItemInitial(ItemInfo itemInfo);
  
  /// <returns>True if item was processed without issue</returns>
  internal bool HandleItem(ItemInfo itemInfo);
}
