using HarmonyLib;
using BepInEx;
using UnityEngine;
using System.Reflection;
using System;

namespace PlayerTrading
{

    [BepInPlugin("projjm.playerTrading", "Player Trading", "1.0.1")]
    public class PlayerTradingMain : BaseUnityPlugin
    {
        internal readonly Harmony harmony = new Harmony("projjm.playerTrading");
        internal Assembly assembly;
        internal static event Action OnLocalPlayerChanged;

        private TradeHandler tradeHandler;

        public void Awake()
        {
            assembly = Assembly.GetExecutingAssembly();
            harmony.PatchAll(assembly);
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

