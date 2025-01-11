namespace RhythmDoctor.Archipelago.Helpers;

internal static class CustomLevelsWardHelper
{
  internal static void CreateCustomTab(
    string label,
    int wardID,
    Action action,
    Sprite? icon = null,
    AudioClip? selectAudio = null
  )
  {
    Plugin.Logger?.LogDebug($"{scnCLS.instance.wardOptions.Count} ward options");
    for (int i = 0; i < scnCLS.instance.wardOptions.Count; i++)
    {
      scnCLS.WardOption debugOptions = scnCLS.instance.wardOptions[i];
      Plugin.Logger?.LogDebug(debugOptions.name);
    }

    // TODO: Using GameObject.Find is a potentially costly method.
    //       If possible, we should cache these results.
    Plugin.Logger?.LogInfo($"Creating custom tab {label}: {wardID}");
    //Assert.IsFalse(Enum.IsDefined(typeof(scnCLS.WardOptionName), wardID), "Cannot use existing WardOptionName");
    GameObject tabTemplate = GameObject.Find("Library Tab").gameObject;
    GameObject tab = UnityEngine.Object.Instantiate(tabTemplate, scnCLS.instance.wardOptionsContainer.transform, false);

    GameObject signContainer = tab.transform.Find("LibrarySign Container").gameObject;
    GameObject signButton = signContainer.transform.Find("Button").gameObject;
    GameObject signIconObject = signButton.transform.Find("Icon Image").gameObject;
    GameObject signLabelObject = signButton.transform.Find("Text").gameObject;

    Text signLabel = signLabelObject.GetComponent<Text>();

    if (icon != null)
    {
      Image signIcon = signIconObject.GetComponent<Image>();
      signIcon.sprite = icon;
    }

    if (selectAudio != null)
    {
      // TODO
    }

    // We could probably localize this by adding our own Key if we needed to
    signLabelObject.GetComponent<RDStringToUIText>().enabled = false;
    signLabel.text = label;

    // Register our WardOption so we can be selected
    scnCLS.WardOption wardOption = scnCLS.instance.wardOptions[0];
    wardOption.name = scnCLS.WardOptionName.Library;
    if (icon is not null)
      wardOption.signImage.sprite = icon;

    Plugin.Logger?.LogDebug($"{scnCLS.instance.wardOptions.Count} ward options");
    for (int i = 0; i < scnCLS.instance.wardOptions.Count; i++)
    {
      scnCLS.WardOption debugOptions = scnCLS.instance.wardOptions[i];
      Plugin.Logger?.LogDebug(debugOptions.name);
    }

    scnCLS.instance.wardOptions.Add(wardOption); // FIXME: Silently failing, can't select ward option
    CustomLevelsWardUIPatch.CustomLevelWardOptions.Add(wardID, action);

    // Register Action to run when user selected option

    Plugin.Logger?.LogDebug($"{scnCLS.instance.wardOptions.Count} ward options");
    for (int i = 0; i < scnCLS.instance.wardOptions.Count; i++)
    {
      scnCLS.WardOption debugOptions = scnCLS.instance.wardOptions[i];
      Plugin.Logger?.LogDebug(debugOptions.name);
    }

    // We do this because we might not have the option selected, leading to
    //  options being selected when they shouldn't
    // By invoking CurrentWardOptionIndex's setter method we resolve this issue
    scnCLS.instance.CurrentWardOptionIndex = scnCLS.instance.CurrentWardOptionIndex;
  }
}
