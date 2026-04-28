namespace RhythmDoctor.Archipelago.Modifiers;

internal abstract class ModifierPatch<T> where T : IModifier
{
  internal Harmony _previewHarmony;
  internal Harmony _activeHarmony;
  
  public abstract Type[]? PreviewPatches { get; }
  public abstract Type[]? ActivePatches { get; }

  public virtual void Initialize()
  {
    _previewHarmony = new($"{Plugin.PATCH_ID_TRAP}.{nameof(T)}.preview");
    _activeHarmony = new($"{Plugin.PATCH_ID_TRAP}.{nameof(T)}.active");
  }

  public virtual void Preview(int strength)
  {
    if (PreviewPatches is null)
      return;
    
    foreach (Type previewPatch in PreviewPatches)
    {
      _previewHarmony.PatchAll(previewPatch);
    }
  }

  public virtual void PreviewEnd()
  {
    _previewHarmony.UnpatchSelf();
  }

  public virtual void Active(int strength)
  {
    if (ActivePatches is null)
      return;
    
    foreach (Type activePatch in ActivePatches)
    {
      _activeHarmony.PatchAll(activePatch);
    }
  }

  public virtual void ActiveEnd()
  {
    _activeHarmony.UnpatchSelf();
  }
}
