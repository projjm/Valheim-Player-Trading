using System.Collections.Generic;
using UnityEngine;

namespace PlayerTrading
{
    public static class HUDTools
    {
        private static GameObject _craftingHUD;
        private static GameObject _infoHUD;
        private static Dictionary<GameObject, bool> _states = new Dictionary<GameObject, bool>();

        public static void SetHUDsActive(bool active)
        {
            if (_craftingHUD == null)
                _craftingHUD = InventoryGui.m_instance.m_repairButton.transform.parent.gameObject;

            if (_infoHUD == null)
                _infoHUD = InventoryGui.m_instance.m_infoPanel.transform.gameObject;

            if (active)
            {
                _craftingHUD.SetActive(true);
                _infoHUD.SetActive(true);
            }
            else
            {
                _craftingHUD.SetActive(false);
                _infoHUD.SetActive(false);
            }
        }

        public static void InventoryGui_ForceShow()
        {
            InventoryGui gui = InventoryGui.m_instance;
            Hud.HidePieceSelection();
            gui.m_animator.SetBool("visible", true);
            gui.SetActiveGroup(1);
            Player localPlayer = Player.m_localPlayer;
            if ((bool)localPlayer)
            {
                gui.SetupCrafting();
            }
            gui.m_currentContainer = null; // Set to null

            gui.m_hiddenFrames = 0;
            if ((bool)localPlayer)
            {
                gui.m_openInventoryEffects.Create(localPlayer.transform.position, Quaternion.identity);
            }
            Gogan.LogEvent("Screen", "Enter", "Inventory", 0L);
        }

        public static void InventoryGui_ForceHide()
        {
            InventoryGui gui = InventoryGui.m_instance;

            if (gui.m_animator.GetBool("visible"))
            {
                gui.m_craftTimer = -1f;
                gui.m_animator.SetBool("visible", false);
                gui.m_trophiesPanel.SetActive(value: false);
                gui.m_variantDialog.gameObject.SetActive(value: false);
                gui.m_skillsDialog.gameObject.SetActive(value: false);
                gui.m_textsDialog.gameObject.SetActive(value: false);
                gui.m_splitPanel.gameObject.SetActive(value: false);
                gui.SetupDragItem(null, null, 1);
                if ((bool)Player.m_localPlayer)
                {
                    gui.m_closeInventoryEffects.Create(Player.m_localPlayer.transform.position, Quaternion.identity);
                }
                Gogan.LogEvent("Screen", "Exit", "Inventory", 0L);
            }
        }

        public static void InventoryGui_ForceCloseContainer()
        {
            InventoryGui gui = InventoryGui.m_instance;
            if (gui.m_dragInventory != null && gui.m_dragInventory != Player.m_localPlayer.GetInventory())
            {
                gui.SetupDragItem(null, null, 1);
            }

            gui.m_splitPanel.gameObject.SetActive(value: false);
            gui.m_firstContainerUpdate = true;
            gui.m_container.gameObject.SetActive(value: false);
        }

    }
}
