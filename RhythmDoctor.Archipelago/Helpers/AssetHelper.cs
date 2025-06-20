namespace RhythmDoctor.Archipelago.Helpers;

internal static class AssetHelper
{
  internal static class AssetType
  {
    internal static class WardIcons
    {
      internal const string NAME = "WardIcons";
      internal const string ARCHIPELAGO = "archipelago.png";
    }
  }

  internal static Sprite LoadSprite(string assetType, string assetName) =>
    LoadSprite(LoadTexture(assetType, assetName));

  internal static Sprite LoadSprite(Texture2D texture2D) =>
    Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero, 1);

  internal static Texture2D LoadTexture(string assetType, string assetName)
  {
    string path = Path.Combine(Paths.Assets, assetType, assetName);

    if (!File.Exists(path))
      throw new FileNotFoundException($"The asset {assetType}/{assetName} was not found");

    byte[] imageBytes = File.ReadAllBytes(path);

    Texture2D texture2D = new(2, 2) { filterMode = FilterMode.Point };
    texture2D.LoadImage(imageBytes);

    return texture2D;
  }
}
