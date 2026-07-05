namespace RhythmDoctor.Archipelago.Client;

internal struct LoginInformation(Mode mode, Uri uri, string slotName, string? password = null)
{
  internal readonly Mode Mode = mode;
  internal readonly Uri Uri = uri;
  internal readonly string SlotName = slotName;
  internal readonly string? Password = password;
}
