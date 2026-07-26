namespace RhythmDoctor.Archipelago.Modifiers.Archipelago.Powerups;

internal class StrongHeartPowerup : ModifierPatch<StrongHeartPowerup>, IModifier, IArchipelagoModifier
{
  /// <summary>
  /// By how much we should reduce mistake weight:
  /// mistake weight = original mistake weight * strength^-1,
  /// where strength is 2*consumed
  /// </summary>
  private static float Strength;

  public string Uid => $"{MyPluginInfo.PLUGIN_GUID}.mod.strongHeart";
  public string LocalizationKey => "mods.archipelago.powerup.strongHeart";

  // TODO: CACHE
  public ModifierCompatibility Compatibility =>
    ModifierCompatibilityBuilder.GetDefaultBuilderForMod(this).SetMaximumStrength(2).Build();
  public ModifierCapability[] Capabilities => [ModifierCapability.HeartStrength];

  public override Type[] PreviewPatches => [];
  public override Type[] ActivePatches => [typeof(ActivePatch)];

  public IScale Scale => new StrongHeartScale();

  public override void Active(float strength)
  {
    base.Active(strength);
    Strength = strength;
  }

  [HarmonyPatch(typeof(MistakesManager))]
  private static class ActivePatch
  {
    [HarmonyPatch(nameof(MistakesManager.AddMistake))]
    [HarmonyPrefix]
    private static void ReduceMistakeWeightPatch(ref float weight)
    {
      weight /= Strength;
    }
  }

  private class StrongHeartScale : IScale
  {
    /// <summary>
    /// How much traps we can consume in one usage.
    /// </summary>
    /// <seealso cref="Strength"/>
    private const int MAX_CONSUMED = 2;

    public float GetScale(int num, out int consumed)
    {
      // this doesn't run if trap weight is 0/no traps in queue
      consumed = Math.Clamp(num, 1, MAX_CONSUMED);
      return num * (2 * consumed) ^ -1;
    }
  }
}
