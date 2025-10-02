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
        public int itemID;
        [SerializeField]
        public int quantity;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref itemID);
            serializer.SerializeValue(ref quantity);
        }
    }
}
