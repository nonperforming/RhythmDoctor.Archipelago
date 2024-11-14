namespace RhythmDoctor.Archipelago.Helpers;

internal static class AssetHelper
{
  internal static Sprite LoadSprite(IAssetType assetType, string assetName) =>
    LoadSprite(LoadTexture(assetType, assetName));

  internal static Sprite LoadSprite(Texture2D texture2D)
  {
    return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero, 1);
  }

  internal static Texture2D LoadTexture(IAssetType assetType, string assetName)
  {
    string path = Path.Combine(Paths.Assets, assetType.GetAssetName(), assetName);

    if (!File.Exists(path))
      throw new FileNotFoundException($"The asset {assetType.GetAssetName()}/{assetName} was not found");

    byte[] imageBytes = File.ReadAllBytes(path);

    Texture2D texture2D = new(2, 2);
    texture2D.LoadImage(imageBytes);

    return texture2D;
  }
}

// TODO: Is there a better way to do this?
internal class WardIcons : IAssetType
{
  public int GetValue() => 1;

  public string GetAssetName() => "wardicons";
}

internal interface IAssetType
{
  public int GetValue();
  public string GetAssetName();
}
