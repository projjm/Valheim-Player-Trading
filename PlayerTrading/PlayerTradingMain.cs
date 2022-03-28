using HarmonyLib;
using BepInEx;
using UnityEngine;
using System.Reflection;
using System;
using BepInEx.Configuration;
using fastJSON;
using System.IO;


namespace PlayerTrading
{
    [Serializable]
    public class StringLocalization
    {
        public string TradeRequestSent = "Trade request sent";
        public string TradeRecentlySent = "Trade request recently sent";
        public string StartedTradeWithX = "Started trading with";
        public string XWantsToTrade = "wants to trade";
        public string XHasCancelledTrade = "has cancelled the trade";
        public string LocalPlayerCancelledTrade = "You have cancelled the trade";
        public string CantStartNewTradeInstance = "You cannot start a new trade session";
        public string TradeSuccessful = "Trade successful";
        public string NotEnoughInventorySlots = "Not enough inventory slots";
        public string ToGiveWindowText = "You Will Give";
        public string ToReceiveWindowText = "You Will Receive";
        public string AcceptTradeButtonText = "Accept Trade";
        public string ChangeTradeButtonText = "Change Trade";
        public string CancelTradeButtonText = "Cancel Trade";
        public string EditModeOn = "Edit UI Mode ON";
        public string EditModeOff = "Edit UI Mode OFF";
    }

    public enum ModifierKey
    {
        NONE,
        CTRL,
        ALT,
    }

    [BepInPlugin("projjm.playerTrading", "Player Trading", "1.2.1")]
    public class PlayerTradingMain : BaseUnityPlugin
    {
        public static ConfigEntry<bool>? UseModifierKey;
        public static ConfigEntry<KeyCode>? ModifierKey;
        public static ConfigEntry<KeyCode>? EditWindowLayoutKey;
        public static ConfigEntry<Vector2>? ToGiveUserOffset;
        public static ConfigEntry<Vector2>? ToReceiveUserOffset;
        public static ConfigEntry<Vector2>? AcceptButtonUserOffset;
        public static ConfigEntry<Vector2>? CancelButtonUserOffset;
        public static StringLocalization? Localization;
        private const string LocalizationFileName = "PlayerTradingStrings.txt";

        internal readonly Harmony harmony = new Harmony("projjm.playerTrading");
        internal Assembly? assembly;
        internal static event Action? OnLocalPlayerChanged;

        private TradeHandler? tradeHandler;

        public void Awake()
        {
            assembly = Assembly.GetExecutingAssembly();
            harmony.PatchAll(assembly);
            BindConfigs();
            InitLocalization();
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

        private void InitLocalization()
        {
            JSON.ClearReflectionCache();
            string filePath = Paths.ConfigPath + Path.PathSeparator;

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            if (File.Exists(filePath + LocalizationFileName))
            {
                string json = File.ReadAllText(filePath + LocalizationFileName);
                Localization = JSON.ToObject<StringLocalization>(json);
            }
            else
            {
                JSONParameters param = new JSONParameters();
                param.UseExtensions = false;

                Localization = new StringLocalization();
                string json = JSON.ToNiceJSON(Localization, param);
                File.WriteAllText(filePath + LocalizationFileName, json);
            }
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

