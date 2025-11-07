using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Lootrun.types
{
    public class LootrunResults : INetworkSerializable
    {
        [SerializeField]
        public List<string> players = new List<string>();
        [SerializeField]
        public float time;
        [SerializeField]
        public Vector2Int scrapCollectedOutOf;
        [SerializeField]
        public LootrunPreset presetUsed = new LootrunPreset();

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            int count = players != null ? players.Count : 0;
            serializer.SerializeValue(ref count);
            if (serializer.IsReader)
            {
                players = new List<string>(count);
                for (int i = 0; i < count; i++)
                {
                    string item = string.Empty;
                    serializer.SerializeValue(ref item);
                    players.Add(item);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    var item = players[i];
                    serializer.SerializeValue(ref item);
                }
            }

            serializer.SerializeValue(ref time);
            serializer.SerializeValue(ref scrapCollectedOutOf);
            serializer.SerializeValue(ref presetUsed);
        }
    }
}
