using HarmonyLib;
using BepInEx;
using UnityEngine;
using System.Reflection;
using System;
using BepInEx.Configuration;
using fastJSON;
using System.IO;
using ServerSync;


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
        internal static ConfigEntry<bool>? ServerConfigLocked = null!;
        public static ConfigEntry<bool>? UseModifierKey;
        public static ConfigEntry<KeyCode>? ModifierKey;
        public static ConfigEntry<KeyCode>? EditWindowLayoutKey;
        public static ConfigEntry<Vector2>? ToGiveUserOffset;
        public static ConfigEntry<Vector2>? ToReceiveUserOffset;
        public static ConfigEntry<Vector2>? AcceptButtonUserOffset;
        public static ConfigEntry<Vector2>? CancelButtonUserOffset;
        public static StringLocalization? Localization;
        private const string LocalizationFileName = "PlayerTradingStrings.txt";
        ConfigSync configSync = new("projjm.playerTrading") 
            { DisplayName = "Player Trading", CurrentVersion = "1.2.1", MinimumRequiredVersion = "1.2.1"};
        internal readonly Harmony harmony = new Harmony("projjm.playerTrading");
        internal Assembly? assembly;
        internal static event Action? OnLocalPlayerChanged;

        private TradeHandler? tradeHandler;

        
        ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        
        
        public void Awake()
        {
            assembly = Assembly.GetExecutingAssembly();
            harmony.PatchAll(assembly);
            BindConfigs();
            InitLocalization();
        }

        private void BindConfigs()
        {
            ServerConfigLocked = config("1 - General", "Lock Configuration", true, "If on, the configuration is locked and can be changed by server admins only.");

            UseModifierKey = config("Keybinds", "useModifierKey", false, "Should sending/receiving trade requests require a modifier key to be held");
            ModifierKey = config("Keybinds", "modifierKey", KeyCode.LeftAlt, "The modifier key that needs to be held if useModifierKey is set to true");
            ToGiveUserOffset = config("Offsets", "toGiveUserOffset", Vector2.zero, "Offset values for To Give trade window (Set to nothing to reset position)");
            ToReceiveUserOffset = config("Offsets", "toReceiveUserOffset", Vector2.zero, "Offset values for To Receive trade window (Set to nothing to reset position)");
            AcceptButtonUserOffset = config("Offsets", "acceptButtonUserOffset", Vector2.zero, "Offset values for the Accept Trade button (Set to nothing to reset position)");
            CancelButtonUserOffset = config("Offsets", "cancelButtonUserOffset", Vector2.zero, "Offset values for the Cancel Trade button (Set to nothing to reset position)");
            EditWindowLayoutKey = config("Keybinds", "editWindowLayoutKey", KeyCode.F11, "Key to press to enable Window Position Mode");
            configSync.AddLockingConfigEntry(ServerConfigLocked);
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

