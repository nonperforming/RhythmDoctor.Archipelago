namespace RhythmDoctor.Archipelago.Helpers;

internal static class CustomLevelsWardUIHelper
{
  static GameObject TabTemplate = scnCLS.instance.transform.Find("Library Tab").gameObject;

  // FIXME: Does this break on scene reloads? (Reference to GameObject is destroyed)
  // Do we need to cache GameObject.Find in the first place? It should not be called often
  //GameObject WardOptionsContainer
  //{
  //  get
  //  {
  //    if (_WardOptionsContainer == null) _WardOptionsContainer = GameObject.Find("WardOptions Container");
  //  }
  //}
  //
  //GameObject _WardOptionsContainer;

  internal static void CreateCustomTab(
    string label,
    int wardID,
    Action action,
    Sprite? icon = null,
    AudioClip? selectAudio = null
  )
  {
    Assert.IsFalse(Enum.IsDefined(typeof(scnCLS.WardOptionName), wardID), "Cannot use existing WardOptionName");

    GameObject tab = UnityEngine.Object.Instantiate(TabTemplate);
    tab.transform.SetParent(scnCLS.instance.wardOptionsContainer.transform, false);

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

    scnCLS.instance.wardOptions.Add(wardOption);
    CustomLevelsWardUIPatch.LevelWardOptions.Add(wardID, action);
    // Register Action to run when user selected option
  }
}
