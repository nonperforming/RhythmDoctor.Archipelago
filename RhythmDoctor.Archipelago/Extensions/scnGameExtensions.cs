namespace RhythmDoctor.Archipelago.Extensions;

internal static partial class scnGameExtensions
{
  internal static void ShakeAllHearts(this scnGame @this, int jitterCount, int jitterAmount)
  {
    foreach (Row row in @this.currentLevel.rows)
    {
      // We need to null check all of these otherwise a silent exception may be thrown at runtime
      row?.ent?.heart?.gameObject.GetOrAddComponent<scrThingShake>().activateThingJitter(jitterCount, jitterAmount);
    }
  }

  internal static void ShakeAllHearts(this scnGame @this, float duration, int amountX, int amountY)
  {
    foreach (Row row in @this.currentLevel.rows)
    {
      // We need to null check all of these otherwise a silent exception may be thrown at runtime
      row?.ent?.heart?.gameObject.GetOrAddComponent<scrThingShake>().activateThingJitter(duration, amountX, amountY);
    }
  }

  internal static void ShakeAllHearts(this scnGame @this, float duration, int amount) =>
    @this.ShakeAllHearts(duration, amount, amount);

  internal static void ShakeAllCharacters(this scnGame @this, int jitterCount, int jitterAmount)
  {
    foreach (Row row in @this.currentLevel.rows)
    {
      // We need to null check all of these otherwise a silent exception may be thrown at runtime
      row?.ent?.character?.gameObject.GetOrAddComponent<scrThingShake>()
        .activateThingJitter(jitterCount, jitterAmount);
    }
  }

  internal static void ShakeAllCharacters(this scnGame @this, float duration, int amountX, int amountY)
  {
    foreach (Row row in @this.currentLevel.rows)
    {
      // We need to null check all of these otherwise a silent exception may be thrown at runtime
      row?.ent?.character?.gameObject.GetOrAddComponent<scrThingShake>()
        .activateThingJitter(duration, amountX, amountY);
    }
  }

  internal static void ShakeAllCharacters(this scnGame @this, float duration, int amount) =>
    @this.ShakeAllCharacters(duration, amount, amount);
}
