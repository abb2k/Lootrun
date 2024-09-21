using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lootrun
{
    [HarmonyPatch(typeof(StartMatchLever), nameof(StartMatchLever.StartGame))]
    internal class StartGamePatch
    {
        [HarmonyPostfix]
        static void StartGameHook()
        {
            LootrunBase.LootrunTime = 0;
        }
    }
}
