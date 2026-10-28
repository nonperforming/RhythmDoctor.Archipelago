namespace RhythmDoctor.Archipelago.Modifiers;

internal abstract class ModifierPatch<T>
  where T : IModifier
{
  private string TName
  {
    get
    {
      if (field is not null)
        return field;
      field = GetType().Name;
      return field;
    }
  }

  internal Harmony _previewHarmony = null!;
  internal Harmony _activeHarmony = null!;

  public abstract Type[] PreviewPatches { get; }
  public abstract Type[] ActivePatches { get; }

  public virtual void Initialize()
  {
    Plugin.Logger.LogDebug($"[{TName}] Initializing");
    _previewHarmony = new($"{nameof(T)}.preview");
    _activeHarmony = new($"{nameof(T)}.active");
  }

  public virtual void Preview(float strength)
  {
    Plugin.Logger.LogDebug($"[{TName}:{nameof(Preview)}] Applying preview patches");
    foreach (Type previewPatch in PreviewPatches)
    {
      _previewHarmony.PatchAll(previewPatch);
    }
  }

  public virtual void PreviewEnd()
  {
    Plugin.Logger.LogDebug($"[{TName}:{nameof(PreviewEnd)}] Unapplying preview patches");
    _previewHarmony.UnpatchSelf();
  }

  public virtual void Active(float strength)
  {
    Plugin.Logger.LogDebug($"[{TName}:{nameof(Active)}] Applying active patches");
    foreach (Type activePatch in ActivePatches)
    {
      _activeHarmony.PatchAll(activePatch);
    }
  }

  public virtual void ActiveEnd()
  {
    Plugin.Logger.LogDebug($"[{TName}:{nameof(ActiveEnd)}] Unapplying active patches");
    _activeHarmony.UnpatchSelf();
  }
}
