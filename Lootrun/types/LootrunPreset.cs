using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Lootrun.types
{
    public class LootrunPreset : INetworkSerializable
    {
        [SerializeField]
        public string presetName = string.Empty;
        [SerializeField]
        public List<LootrunItemQuantity> items = new List<LootrunItemQuantity>();
        [SerializeField]
        public int moon;
        [SerializeField]
        public int weatherType = -2;
        [SerializeField]
        public bool countBees = true;
        [SerializeField]
        public bool cruiserOnStart;
        [SerializeField]
        public bool countSpecials;
        [SerializeField]
        public bool isEndless = true;
        [SerializeField]
        public int seed = -1;
        [SerializeField]
        public int money = 0;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref presetName);

            int count = items != null ? items.Count : 0;
            serializer.SerializeValue(ref count);
            if (serializer.IsReader)
            {
                items = new List<LootrunItemQuantity>(count);
                for (int i = 0; i < count; i++)
                {
                    var item = new LootrunItemQuantity();
                    serializer.SerializeValue(ref item);
                    items.Add(item);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    if (items.Count == i) break;

                    var item = items[i];

                    if (item == null) continue;

                    serializer.SerializeValue(ref item);
                }
            }

            serializer.SerializeValue(ref moon);
            serializer.SerializeValue(ref weatherType);
            serializer.SerializeValue(ref countBees);
            serializer.SerializeValue(ref cruiserOnStart);
            serializer.SerializeValue(ref countSpecials);
            serializer.SerializeValue(ref isEndless);
            serializer.SerializeValue(ref seed);
            serializer.SerializeValue(ref money);
        }
    }
}
