namespace RhythmDoctor.Archipelago.Patches.Gameplay;

using Newtonsoft.Json.Linq;

/// <seealso cref="ClientOld"/>
[HarmonyPatch(typeof(Persistence))]
internal static class StateReplicationPatch
{
  // FIXME: Make async and retry on failure!!

  /// <seealso cref="ClientOld.ReplicatePaigeStays"/>
  [HarmonyPatch(nameof(Persistence.SetPaigeEnding))]
  [HarmonyPostfix]
  private static void ReplicatePaigeStaysPatch(bool paigeStays)
  {
    Plugin.Logger.LogInfo($"Replicating {Persistence.PaigeStaysKey} to {paigeStays}");
    Plugin.ClientOld.Session!.DataStorage[Scope.Slot, Persistence.PaigeStaysKey] = paigeStays;
  }

  /// <seealso cref="ClientOld.ReplicateIansDesktopUnlocked"/>
  [HarmonyPatch(nameof(Persistence.SetIanDesktopLogin))]
  [HarmonyPostfix]
  private static void ReplicateIansDesktopUnlockedPatch(bool ianDesktopLogin)
  {
    Plugin.Logger.LogInfo($"Replicating {Persistence.IanDesktopLoginKey} to {ianDesktopLogin}");
    Plugin.ClientOld.Session!.DataStorage[Scope.Slot, Persistence.IanDesktopLoginKey] = ianDesktopLogin;
  }

  internal static async Task InitializeSync()
  {
    static async Task InitializeKey<T>(string key, Action<T> executeExisting, Func<T> valueDoesntExist)
    {
      // Manual implementation of asynchronous DataStorageElement.Initialize() here to avoid timeout errors
      // TODO: retry multiple times if we timeout
      // cannot use T? as runtime will fail with 'Can not convert Null to T.'
      JToken valueRaw = await Plugin.ClientOld.Session!.DataStorage[Scope.Slot, key].GetAsync();

      if (valueRaw.Type != JTokenType.Null && valueRaw.Type != JTokenType.Undefined)
      {
        T value = valueRaw.ToObject<T>()!;
        Plugin.Logger.LogInfo($"Found {key} state: {value}");
        executeExisting.Invoke(value);
      }
      // TODO: Would love to have DataStorage set here.
      //       [CS0029] Cannot implicitly convert type 'T' to 'Archipelago.MultiClient.Net.Models.DataStorageElement'
      else
      {
        T initializeTo = valueDoesntExist.Invoke();
        Plugin.Logger.LogInfo($"Setting initial {key} state to {initializeTo}");
      }
    }

    Task paigeStaysTask = InitializeKey<bool>(
      Persistence.PaigeStaysKey,
      Persistence.SetPaigeEnding,
      () =>
      {
        bool initialPaigeStaysValue = Plugin.Random.Next() % 2 == 1;
        Plugin.ClientOld.Session!.DataStorage[Scope.Slot, Persistence.PaigeStaysKey] = initialPaigeStaysValue;
        return initialPaigeStaysValue;
      }
    );
    Task iansDesktopUnlockedTask = InitializeKey<bool>(
      Persistence.IanDesktopLoginKey,
      Persistence.SetIanDesktopLogin,
      () =>
      {
        Plugin.ClientOld.Session!.DataStorage[Scope.Slot, Persistence.IanDesktopLoginKey] = false;
        return false;
      }
    );

    Plugin.Logger.LogInfo("Waiting for initialization to complete...");
    await Task.WhenAll(paigeStaysTask, iansDesktopUnlockedTask);
    Plugin.Logger.LogInfo("Initialization completed!");
  }
}
