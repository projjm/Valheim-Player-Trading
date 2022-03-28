using UnityEngine;
using HarmonyLib;

namespace PlayerTrading.Patches
{
    class PlayerPatches
    {
        [HarmonyPatch(typeof(Player), nameof(Player.GetHoverText))]
        public class HoverFix
        {
            public static void Postfix(Player __instance, ref string __result)
            {
                if (TradeHandler.Instance.IsTradeWindowsOpen() || __instance == Player.m_localPlayer)
                    return;

                string action = TradeHandler.Instance.GetAction(__instance);
                __result = Localization.instance.Localize(__instance.GetPlayerName() + "\n[<color=yellow><b>$KEY_Use</b></color>] " + action);
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Interact))]
        public class CancelTradeOnAnyInteract
        {
            public static void Postfix(GameObject go)
            {
                if (!TradeHandler.Instance.IsTradeWindowsOpen())
                    return;

                Interactable componentInParent = go.GetComponentInParent<Interactable>();
                if (componentInParent != null)
                {
                    TradeHandler.Instance.TryCancelTradeInstance();
                    TradeHandler.Instance.TryCancelWindowEditMode();
                }

            }

        }

        [HarmonyPatch(typeof(Player), nameof(Player.SetLocalPlayer))]
        public class PlayerListenerPatch
        {
            public static void Postfix()
            {
                PlayerTradingMain.NewLocalPlayer();
            }
        }

    }
}
