using Unity.Netcode;
using UnityEngine;

namespace Lootrun.types
{
    public class LootrunResults : INetworkSerializable
    {
        [SerializeField]
        public int players;
        [SerializeField]
        public float time;
        [SerializeField]
        public Vector2Int scrapCollectedOutOf;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref players);
            serializer.SerializeValue(ref time);
            serializer.SerializeValue(ref scrapCollectedOutOf);
        }
    }
}
