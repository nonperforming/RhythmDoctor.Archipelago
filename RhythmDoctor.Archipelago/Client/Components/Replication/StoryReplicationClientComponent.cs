using Newtonsoft.Json.Linq;

namespace RhythmDoctor.Archipelago.Client.Components;

internal sealed class StoryReplicationClientComponent : ReplicationClientComponent
{
  public IEnumerable<Type> AssistPatches => [typeof(StoryModeStateReplicationPatch)];

  internal override async Task Enable(ArchipelagoSession session)
  {
    static async Task InitializeSync(ArchipelagoSession session)
    {
      static async Task InitializeKey<T>(
        ArchipelagoSession session,
        string key,
        Action<T> executeExisting,
        Func<T> valueDoesntExist
      )
      {
        // Manual implementation of asynchronous DataStorageElement.Initialize() here to avoid timeout errors
        // TODO: retry multiple times if we timeout
        // cannot use T? as runtime will fail with 'Can not convert Null to T.'
        JToken valueRaw = await session.DataStorage[Scope.Slot, key].GetAsync();

        if (valueRaw.Type != JTokenType.Null && valueRaw.Type != JTokenType.Undefined)
        {
          T value = valueRaw.ToObject<T>()!;
          Plugin.Logger.LogInfo($"[{nameof(StoryReplicationClientComponent)}] Found {key} state: {value}");
          executeExisting.Invoke(value);
        }
        // TODO: Would love to have DataStorage set here.
        //       [CS0029] Cannot implicitly convert type 'T' to 'Archipelago.MultiClient.Net.Models.DataStorageElement'
        else
        {
          T initializeTo = valueDoesntExist.Invoke();
          Plugin.Logger.LogInfo(
            $"[{nameof(StoryReplicationClientComponent)}] Setting initial {key} state to {initializeTo}"
          );
        }
      }

      Task paigeStaysTask = InitializeKey<bool>(
        session,
        Persistence.PaigeStaysKey,
        Persistence.SetPaigeEnding,
        () =>
        {
          bool initialPaigeStaysValue = Plugin.Random.Next() % 2 == 1;
          session.DataStorage[Scope.Slot, Persistence.PaigeStaysKey] = initialPaigeStaysValue;
          return initialPaigeStaysValue;
        }
      );
      Task iansDesktopUnlockedTask = InitializeKey<bool>(
        session,
        Persistence.IanDesktopLoginKey,
        Persistence.SetIanDesktopLogin,
        () =>
        {
          session.DataStorage[Scope.Slot, Persistence.IanDesktopLoginKey] = false;
          return false;
        }
      );

      Plugin.Logger.LogInfo($"[{nameof(StoryReplicationClientComponent)}] Waiting for initialization to complete...");
      await Task.WhenAll(paigeStaysTask, iansDesktopUnlockedTask);
      Plugin.Logger.LogInfo($"[{nameof(StoryReplicationClientComponent)}] Initialization completed!");
    }

    await base.Enable(session);
    await InitializeSync(session);
    session.DataStorage[Scope.Slot, Persistence.PaigeStaysKey].OnValueChanged += ReplicatePaigeStays;
    session.DataStorage[Scope.Slot, Persistence.IanDesktopLoginKey].OnValueChanged += ReplicateIansDesktopUnlocked;
  }

  private static void ReplicatePaigeStays(JToken oldValue, JToken newValue, Dictionary<string, JToken> _)
  {
    Plugin.Logger.LogInfo($"[{nameof(StoryReplicationClientComponent)}] Paige stays {oldValue}->{newValue}");
    Persistence.SetPaigeEnding(newValue.ToObject<bool>());
  }

  private static void ReplicateIansDesktopUnlocked(JToken oldValue, JToken newValue, Dictionary<string, JToken> _)
  {
    Plugin.Logger.LogInfo($"[{nameof(StoryReplicationClientComponent)}] Ian's desktop unlocked {oldValue}->{newValue}");
    Persistence.SetIanDesktopLogin(newValue.ToObject<bool>());
  }
}
