using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lootrun.hooks
{
    [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.SaveGame))]
    internal class SaveGamePatch
    {
        [HarmonyPrefix]
        static bool SaveGameHook()
        {
            if (GameNetworkManager.Instance.currentSaveFileName == "Speedloot")
            {
                GameNetworkManager.Instance.currentSaveFileName = "LCSaveFile1";
                return false;
            }

            return true;
        }
    }
}
