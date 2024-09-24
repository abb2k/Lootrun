using Lootrun.types;
using System;
using Unity.Netcode;

namespace Lootrun.hooks
{
    public class LootrunNetworkHandler : NetworkBehaviour
    {

        public static LootrunNetworkHandler Instance { get; private set; }

        public static event Action<String> LevelEvent;
        public static event Action<float> TimeEvent;
        public static event Action<LootrunSettings, LootrunResults> LootrunResEvent;

        public override void OnNetworkSpawn()
        {
            LevelEvent = null;

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            Instance = this;

            base.OnNetworkSpawn();
        }

        [ClientRpc] 
        public void EventClientRpc(string eventName)
        {
            LevelEvent?.Invoke(eventName); // If the event has subscribers (does not equal null), invoke the event
        }

        [ClientRpc]
        public void UpdateTimeClientRpc(float time)
        {
            TimeEvent?.Invoke(time); // If the event has subscribers (does not equal null), invoke the event
        }

        [ClientRpc]
        public void LootrunResultsClientRpc(LootrunSettings settings, LootrunResults results)
        {
            LootrunResEvent?.Invoke(settings, results); // If the event has subscribers (does not equal null), invoke the event
        }
    }
}
