using HarmonyLib;
using PlayerTrading.GUI;
using UnityEngine;

namespace PlayerTrading.Patches
{
	class InventoryGuiPatches
	{

		[HarmonyPriority(9999)]
		[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateGamepad))]
		class GamepadSupportTrade
		{
			public static bool Prefix(InventoryGui __instance)
			{
				if (__instance.m_currentContainer != null || (!TradeHandler.Instance || !TradeHandler.Instance.IsTradeWindowsOpen()))
					return true;

				if (__instance.m_inventoryGroup.IsActive())
				{
					if (ZInput.GetButtonDown("JoyTabLeft"))
					{
						__instance.SetActiveGroup(1);
					}
					if (ZInput.GetButtonDown("JoyTabRight"))
					{
						__instance.SetActiveGroup(0);
					}
					if (__instance.m_activeGroup != 0 && __instance.m_activeGroup != 1)
					{
						__instance.SetActiveGroup(1);
					}
				}

				return false;
			}
		}

		[HarmonyPriority(9999)]
		[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Show))]
		class ShowHUDExtraElementsFix
		{
			public static void Prefix()
			{
				if (!TradeHandler.Instance.IsTradeWindowsOpen())
				{
					HUDTools.SetHUDsActive(true);
					InventoryGui.instance.m_animator.speed = 1f;
				}
			}
		}


		[HarmonyPriority(9999)]
		[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Update))]
		public class InteractCancelTradeFix
		{
			public static void Prefix()
			{
				if (TradeHandler.Instance && TradeHandler.Instance.IsTradeWindowsOpen())
					ZInput.ResetButtonStatus("Use"); // Prevent Use from closing trade instance
			}
		}


		[HarmonyPriority(9999)]
		[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Hide))]
		class HideHUDCancelTrade
		{
			public static void Prefix()
			{
				if (TradeHandler.Instance.IsTradeWindowsOpen())
				{
					TradeHandler.Instance.TryCancelTradeInstance();
					TradeHandler.Instance.TryCancelWindowEditMode();
				}
			}
		}

		[HarmonyPriority(9999)]
		[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateContainerWeight))]
        class PatchUpdateContainerWeight
        {
            public static bool Prefix(InventoryGui __instance)
            {
				if (__instance.m_currentContainer != null || !TradeHandler.Instance.IsTradeWindowsOpen())
                    return true;

				Inventory inv;
				if (TradeHandler.Instance.InTradeInstance())
					inv = TradeHandler.Instance.TryGetToTradeInventory();
				else
					inv = TradeWindowManager.Instance.GetToTradeInventory();

				int num = Mathf.CeilToInt(inv.GetTotalWeight());
                __instance.m_containerWeight.text = num.ToString();

                return false;

            }
        }

		[HarmonyPriority(9999)]
		[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateContainer))]
        class PatchUpdateContainer
        {
            public static bool Prefix(InventoryGui __instance)
            {
                if (__instance.m_currentContainer != null || !TradeHandler.Instance.IsTradeWindowsOpen())
                    return true;

				Inventory inv;
				if (TradeHandler.Instance.InTradeInstance())
					inv = TradeHandler.Instance.TryGetToTradeInventory();
				else
					inv = TradeWindowManager.Instance.GetToTradeInventory();

				if (!__instance.m_animator.GetBool("visible"))
                {
                    return false;
                }

                __instance.m_container.gameObject.SetActive(value: true);
                __instance.m_containerGrid.UpdateInventory(inv, null, __instance.m_dragItem);
                __instance.m_containerName.text = inv.GetName();
                if (__instance.m_firstContainerUpdate)
                {
                    __instance.m_containerGrid.ResetView();
                    __instance.m_firstContainerUpdate = false;
                }

                return false;

            }
        }

		[HarmonyPriority(9999)]
		[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnTakeAll))]
        class PatchOnTakeAll
        {
            public static bool Prefix(InventoryGui __instance)
            {
                if (__instance.m_currentContainer != null || !TradeHandler.Instance.IsTradeWindowsOpen())
                    return true;

				Inventory inv;
				if (TradeHandler.Instance.InTradeInstance())
					inv = TradeHandler.Instance.TryGetToTradeInventory();
				else
					inv = TradeWindowManager.Instance.GetToTradeInventory();

				if (!Player.m_localPlayer.IsTeleporting())
                {
                    __instance.SetupDragItem(null, null, 1);
                    Inventory inventory = inv;
                    Player.m_localPlayer.GetInventory().MoveAll(inventory);
                }

                return false;
            }
        }


		[HarmonyPriority(9999)]
		[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnSelectedItem))]
        class PatchOnSelectedItem
        {
            public static bool Prefix(InventoryGui __instance, InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos, InventoryGrid.Modifier mod)
            {
                if (__instance.m_currentContainer != null || !TradeHandler.Instance.IsTradeWindowsOpen())
                    return true;

				Inventory inv;
				if (TradeHandler.Instance.InTradeInstance())
					inv = TradeHandler.Instance.TryGetToTradeInventory();
				else
					inv = TradeWindowManager.Instance.GetToTradeInventory();

				Player localPlayer = Player.m_localPlayer;
				if (localPlayer.IsTeleporting())
				{
					return false;
				}
				if ((bool)__instance.m_dragGo)
				{
					__instance.m_moveItemEffects.Create(__instance.transform.position, Quaternion.identity);
					bool flag = localPlayer.IsItemEquiped(__instance.m_dragItem);
					bool flag2 = item != null && localPlayer.IsItemEquiped(item);
					Vector2i gridPos = __instance.m_dragItem.m_gridPos;
					if ((__instance.m_dragItem.m_shared.m_questItem || (item != null && item.m_shared.m_questItem)) && __instance.m_dragInventory != grid.GetInventory())
					{
						return false;
					}
					if (!__instance.m_dragInventory.ContainsItem(__instance.m_dragItem))
					{
						__instance.SetupDragItem(null, null, 1);
						return false;
					}
					localPlayer.RemoveFromEquipQueue(item);
					localPlayer.RemoveFromEquipQueue(__instance.m_dragItem);
					localPlayer.UnequipItem(__instance.m_dragItem, triggerEquipEffects: false);
					localPlayer.UnequipItem(item, triggerEquipEffects: false);
					bool num = grid.DropItem(__instance.m_dragInventory, __instance.m_dragItem, __instance.m_dragAmount, pos);
					if (__instance.m_dragItem.m_stack < __instance.m_dragAmount)
					{
						__instance.m_dragAmount = __instance.m_dragItem.m_stack;
					}
					if (flag)
					{
						ItemDrop.ItemData itemAt = grid.GetInventory().GetItemAt(pos.x, pos.y);
						if (itemAt != null)
						{
							localPlayer.EquipItem(itemAt, triggerEquipEffects: false);
						}
						if (localPlayer.GetInventory().ContainsItem(__instance.m_dragItem))
						{
							localPlayer.EquipItem(__instance.m_dragItem, triggerEquipEffects: false);
						}
					}
					if (flag2)
					{
						ItemDrop.ItemData itemAt2 = __instance.m_dragInventory.GetItemAt(gridPos.x, gridPos.y);
						if (itemAt2 != null)
						{
							localPlayer.EquipItem(itemAt2, triggerEquipEffects: false);
						}
						if (localPlayer.GetInventory().ContainsItem(item))
						{
							localPlayer.EquipItem(item, triggerEquipEffects: false);
						}
					}
					if (num)
					{
						__instance.SetupDragItem(null, null, 1);
						__instance.UpdateCraftingPanel();
					}
				}
				else
				{
					if (item == null)
					{
						return false;
					}
					switch (mod)
					{
						case InventoryGrid.Modifier.Move:
							if (item.m_shared.m_questItem)
							{
								return false;
							}

							localPlayer.RemoveFromEquipQueue(item);
							localPlayer.UnequipItem(item);
							if (grid.GetInventory() == inv)
							{
								localPlayer.GetInventory().MoveItemToThis(grid.GetInventory(), item);
							}
							else
							{
								inv.MoveItemToThis(localPlayer.GetInventory(), item);
							}
							__instance.m_moveItemEffects.Create(__instance.transform.position, Quaternion.identity);
							
							return false;
						case InventoryGrid.Modifier.Split:
							if (item.m_stack > 1)
							{
								__instance.ShowSplitDialog(item, grid.GetInventory());
								return false;
							}
							break;
					}

					__instance.SetupDragItem(item, grid.GetInventory(), item.m_stack);
				}

				return false;
            }
        }

		[HarmonyPriority(9999)]
		[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.IsContainerOpen))]
		class PatchIsContainerOpen
		{
			public static bool Prefix(InventoryGui __instance, ref bool __result)
			{
				if (__instance.m_currentContainer != null || !TradeHandler.Instance.IsTradeWindowsOpen())
					return true;

				__result = true;

				return false;
			}
		}
	}
}
