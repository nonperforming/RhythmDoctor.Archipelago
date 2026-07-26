namespace RhythmDoctor.Archipelago.Modifiers.Archipelago.Traps;

internal class FragileHeartTrap : ModifierPatch<FragileHeartTrap>, IModifier, IArchipelagoModifier
{
  /// <summary>
  /// By how much we should increase mistake weight:
  /// mistake weight = original mistake weight * strength,
  /// where strength is 2*consumed
  /// </summary>
  private static float Strength;

  public string Uid => $"{MyPluginInfo.PLUGIN_GUID}.mod.fragileHeart";
  public string LocalizationKey => "mods.archipelago.trap.fragileHeart";

  // TODO: CACHE
  public ModifierCompatibility Compatibility =>
    ModifierCompatibilityBuilder.GetDefaultBuilderForMod(this).SetMaximumStrength(2).Build();
  public ModifierCapability[] Capabilities => [ModifierCapability.HeartStrength];

  public override Type[] PreviewPatches => [];
  public override Type[] ActivePatches => [typeof(ActivePatch)];

  public IScale Scale => new FragileHeartScale();

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
    private static void IncreaseMistakeWeightPatch(ref float weight)
    {
      weight *= Strength;
    }
  }

  private class FragileHeartScale : IScale
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
      return num * (2 * consumed);
    }
  }
}
