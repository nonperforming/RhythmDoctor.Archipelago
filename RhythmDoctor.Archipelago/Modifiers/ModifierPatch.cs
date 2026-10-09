namespace RhythmDoctor.Archipelago.Modifiers;

internal abstract class ModifierPatch<T>
  where T : IModifier
{
  internal Harmony _previewHarmony = null!;
  internal Harmony _activeHarmony = null!;

  public abstract Type[] PreviewPatches { get; }
  public abstract Type[] ActivePatches { get; }

  public virtual void Initialize()
  {
    Plugin.Logger.LogDebug($"[{nameof(T)}] Initializing");
    _previewHarmony = new($"{Plugin.PATCH_ID_TRAP}.{nameof(T)}.preview");
    _activeHarmony = new($"{Plugin.PATCH_ID_TRAP}.{nameof(T)}.active");
  }

  public virtual void Preview(float strength)
  {
    Plugin.Logger.LogDebug($"[{nameof(T)}] Preview; applying preview patches");
    foreach (Type previewPatch in PreviewPatches)
    {
      _previewHarmony.PatchAll(previewPatch);
    }
  }

  public virtual void PreviewEnd()
  {
    Plugin.Logger.LogDebug($"[{nameof(T)}] PreviewEnd; unapplying preview patches");
    _previewHarmony.UnpatchSelf();
  }

  public virtual void Active(float strength)
  {
    Plugin.Logger.LogDebug($"[{nameof(T)}] Active; applying active patches");
    foreach (Type activePatch in ActivePatches)
    {
      _activeHarmony.PatchAll(activePatch);
    }
  }

  public virtual void ActiveEnd()
  {
    Plugin.Logger.LogDebug($"[{nameof(T)}] ActiveEnd; unapplying active patches");
    _activeHarmony.UnpatchSelf();
  }
}
