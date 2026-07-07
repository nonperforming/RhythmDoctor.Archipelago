namespace RhythmDoctor.Archipelago.Client.Components.ItemProcessors;

internal class TrapItemProcessorClientComponent : ItemProcessorClientComponent
{
  internal override Task Enable(ArchipelagoSession session)
  {
    return Task.CompletedTask;
    //throw new NotImplementedException();
  }

  internal override bool HandleItemInitial(ItemInfo itemInfo)
  {
    return false;
    //throw new NotImplementedException();
  }

  internal override bool HandleItem(ItemInfo itemInfo)
  {
    return false;
    //throw new NotImplementedException();
  }
}
