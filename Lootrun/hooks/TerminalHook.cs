using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lootrun.hooks
{
    [HarmonyPatch(typeof(Terminal), "LoadNewNodeIfAffordable")]
    internal class TextPostProcessHook
    {
        [HarmonyPrefix]
        static bool patch(object[] __args, Terminal __instance)
        {
            TerminalNode node = (TerminalNode)__args[0];

            if (node.buyRerouteToMoon != -1 && node.buyRerouteToMoon != -2)
            {
                if (LootrunBase.isInLootrun)
                {
                    TerminalNode n = new TerminalNode();
                    n.displayText = "You can change planets while on a lootrun!\n";
                    n.clearPreviousText = true;
                    n.playClip = __instance.terminalNodes.specialNodes[24].playClip;
                    n.playSyncedClip = __instance.terminalNodes.specialNodes[24].playSyncedClip;
                    __instance.LoadNewNode(n);
                    return false;
                }
            }

            return true;
        }
    }
}
