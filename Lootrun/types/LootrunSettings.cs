using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Lootrun.types
{
    [SerializeField]
    public class LootrunSettings : INetworkSerializable
    {
        [SerializeField]
        public int moon;
        [SerializeField]
        public int weather;
        [SerializeField]
        public bool bees = true;
        [SerializeField]
        public bool spacials;
        [SerializeField]
        public bool randomseed = true;
        [SerializeField]
        public int seed;
        [SerializeField]
        public int money;
        [SerializeField]
        public bool startCrusier;
        [SerializeField]
        public bool startJetpack;

        public bool compare(LootrunSettings other)
        {
            if (other == null) return false;

            if (moon != other.moon) return false;
            if (weather != other.weather) return false;
            if (bees != other.bees) return false;
            if (spacials != other.spacials) return false;
            if (randomseed != other.randomseed) return false;
            if (seed != other.seed && randomseed) return false;
            if (money != other.money) return false;
            if (startCrusier != other.startCrusier) return false;
            if (startJetpack != other.startJetpack) return false;

            return true;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref moon);
            serializer.SerializeValue(ref weather);
            serializer.SerializeValue(ref bees);
            serializer.SerializeValue(ref spacials);
            serializer.SerializeValue(ref randomseed);
            serializer.SerializeValue(ref seed);
            serializer.SerializeValue(ref money);
            serializer.SerializeValue(ref startJetpack);
            serializer.SerializeValue(ref startCrusier);
        }
    }
}
