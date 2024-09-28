using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Lootrun.types
{
    [SerializeField]
    public class Pair<T, U>
    {
        public Pair()
        {
        }

        public Pair(T first, U second)
        {
            this.First = first;
            this.Second = second;
        }

        [SerializeField]
        public T First { get; set; }
        [SerializeField]
        public U Second { get; set; }
    };
}
