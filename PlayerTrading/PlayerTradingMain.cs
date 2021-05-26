using HarmonyLib;
using BepInEx;
using UnityEngine;
using System.Reflection;
using System;
using BepInEx.Configuration;

namespace PlayerTrading
{
    public enum ModifierKey
    {
        NONE,
        CTRL,
        ALT,

    }

    [BepInPlugin("projjm.playerTrading", "Player Trading", "1.1.1")]
    public class PlayerTradingMain : BaseUnityPlugin
    {
        public static ConfigEntry<bool> UseModifierKey;
        public static ConfigEntry<KeyCode> ModifierKey;
        public static ConfigEntry<KeyCode> EditWindowLayoutKey;
        public static ConfigEntry<Vector2> ToGiveUserOffset;
        public static ConfigEntry<Vector2> ToReceiveUserOffset;
        public static ConfigEntry<Vector2> AcceptButtonUserOffset;
        public static ConfigEntry<Vector2> CancelButtonUserOffset;

        
        internal readonly Harmony harmony = new Harmony("projjm.playerTrading");
        internal Assembly assembly;
        internal static event Action OnLocalPlayerChanged;

        private TradeHandler tradeHandler;

        public void Awake()
        {
            assembly = Assembly.GetExecutingAssembly();
            harmony.PatchAll(assembly);
            BindConfigs();
        }

        private void BindConfigs()
        {
            UseModifierKey = Config.Bind("Keybinds", "useModifierKey", false, "Should sending/receiving trade requests require a modifier key to be held");
            ModifierKey = Config.Bind("Keybinds", "modifierKey", KeyCode.LeftAlt, "The modifier key that needs to be held if useModifierKey is set to true");
            ToGiveUserOffset = Config.Bind("Offsets", "toGiveUserOffset", Vector2.zero, "Offset values for To Give trade window (Set to nothing to reset position)");
            ToReceiveUserOffset = Config.Bind("Offsets", "toReceiveUserOffset", Vector2.zero, "Offset values for To Receive trade window (Set to nothing to reset position)");
            AcceptButtonUserOffset = Config.Bind("Offsets", "acceptButtonUserOffset", Vector2.zero, "Offset values for the Accept Trade button (Set to nothing to reset position)");
            CancelButtonUserOffset = Config.Bind("Offsets", "cancelButtonUserOffset", Vector2.zero, "Offset values for the Cancel Trade button (Set to nothing to reset position)");
            EditWindowLayoutKey = Config.Bind("Keybinds", "editWindowLayoutKey", KeyCode.F11, "Key to press to enable Window Position Mode");
        }

        public void OnDestroy()
        {
            if (tradeHandler)
                Destroy(tradeHandler);

            harmony.UnpatchSelf();
        }

        private void Update()
        {
            TryAddHandler();
        }

        private void TryAddHandler()
        {
            if (tradeHandler != null || !ZNet.instance)
                return;

            GameObject PlayerTrading = new GameObject();
            tradeHandler = PlayerTrading.AddComponent<TradeHandler>();
        }

        public static void NewLocalPlayer()
        {
            OnLocalPlayerChanged?.Invoke();
        }


    }
}

