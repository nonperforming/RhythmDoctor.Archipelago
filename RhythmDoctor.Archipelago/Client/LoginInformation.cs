namespace RhythmDoctor.Archipelago.Client;

internal struct LoginInformation(Mode mode, string uri, string slotName, string? password = null)
{
  internal readonly Mode Mode = mode;
  internal readonly string Uri = uri;
  internal readonly string SlotName = slotName;
  internal readonly string? Password = password;
}
