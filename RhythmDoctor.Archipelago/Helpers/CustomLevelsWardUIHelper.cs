namespace RhythmDoctor.Archipelago.Helpers;

internal static class CustomLevelsWardUIHelper
{
  internal static void CreateCustomTab(
    string label,
    int wardID,
    Action action,
    Sprite? icon = null,
    AudioClip? selectAudio = null
  )
  {
    // TODO: Using GameObject.Find is a potentially costly method.
    //       If possible, we should cache these results.
    Plugin.Logger?.LogInfo($"Creating custom tab {label}: {wardID}");
    Assert.IsFalse(Enum.IsDefined(typeof(scnCLS.WardOptionName), wardID), "Cannot use existing WardOptionName");
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
    //  but Archipelago doesn't *really* support different languages in the first
    //  place (i.e. when sending unlocked item messages)
    signLabelObject.GetComponent<RDStringToUIText>().enabled = false;
    signLabel.text = label;

    // Register our WardOption so we can get inputs
    scnCLS.WardOption wardOption = scnCLS.instance.wardOptions[0];
    wardOption.name = (scnCLS.WardOptionName)wardID;

    scnCLS.instance.wardOptions.Add(wardOption); // FIXME: Silently failing, can't select ward option
    CustomLevelsWardUIPatch.LevelWardOptions.Add(wardID, action);
    // Register Action to run when user selected option
  }
}
