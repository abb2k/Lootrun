using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Lootrun.types
{
    public class LootrunItemQuantity : INetworkSerializable
    {
        [SerializeField]
        public string itemName = string.Empty;
        [SerializeField]
        public int quantity;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref itemName);
            serializer.SerializeValue(ref quantity);
        }
    }
}
