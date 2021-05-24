using HarmonyLib;

namespace PlayerTrading.Patches
{
    class InventoryPatches
    {
		[HarmonyPatch(typeof(Inventory), nameof(Inventory.Changed))]
		class PatchIsContainerOpen
		{
			public static void Postfix(Inventory __instance)
			{
				if (TradeHandler.Instance == null || !TradeHandler.Instance.IsTradeWindowsOpen())
					return;

				if (__instance == TradeHandler.Instance.TryGetToTradeInventory())
					TradeHandler.Instance.NotifyInventoryChanged();

			}
		}
	}
}
