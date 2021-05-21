using PlayerTrading.GUI;
using UnityEngine;

namespace PlayerTrading
{
    class TradeInstance : MonoBehaviour
    {
        private Player _localPlayer;
        private Player _otherPlayer;
        private Transform _localPlayerTransform;
        private Transform _otherPlayerTransform;

        private Inventory _toTrade;
        private Inventory _toReceive;

        private float _maxDistance;
        private bool _cancelledByOtherPlayer;
        private bool _tradeFinalized;
        private bool _hasAccepted;

        private bool HasAccepted
        {
            get { return _hasAccepted; }
            set
            {
                _hasAccepted = value;
                TradeWindowManager.Instance.SetToTradeAccepted(value);
            }
        }

        private bool _otherPlayerhasAccepted;
        private bool OtherPlayerHasAccepted
        {
            get { return _otherPlayerhasAccepted; }
            set
            {
                _otherPlayerhasAccepted = value;
                TradeWindowManager.Instance.SetToReceiveAccepted(value);
            }
        }

        private void Awake() 
        {
            MessageHud.instance.HideAll();
            _toTrade = TradeWindowManager.Instance.GetToTradeInventory();
            _toReceive = TradeWindowManager.Instance.GetToReceiveInventory();
            RegisterRPCs();
            SubscribeToGuiEvents();    
        }

        private void StoreReferences()
        {
            _localPlayerTransform = _localPlayer.transform;
            _otherPlayerTransform = _otherPlayer.transform;
            _maxDistance = TradeHandler.Instance.GetMaxDistance();
        }

        private void SubscribeToGuiEvents()
        {
            TradeWindowManager.Instance.OnTradeAcceptPressed += AcceptTrade;
            TradeWindowManager.Instance.OnCancelTradePressed += DestroyInstance;
            TradeWindowManager.Instance.OnChangeTradePressed += ChangeTrade;
            PlayerTradingMain.OnLocalPlayerChanged += OnNewLocalPlayer;
        }

        private void UnsubscribeToGuiEvents()
        {
            TradeWindowManager.Instance.OnTradeAcceptPressed -= AcceptTrade;
            TradeWindowManager.Instance.OnCancelTradePressed -= DestroyInstance;
            TradeWindowManager.Instance.OnChangeTradePressed -= ChangeTrade;
            PlayerTradingMain.OnLocalPlayerChanged -= OnNewLocalPlayer;
        }

        private void RegisterRPCs()
        {
            ZNetUtils.UnregisterRPC("SendTradeData");
            ZRoutedRpc.instance.Register<ZPackage>("SendTradeData", RPC_ReceiveTradeData);
            ZNetUtils.UnregisterRPC("AcceptTrade");
            ZRoutedRpc.instance.Register("AcceptTrade", RPC_AcceptTrade);
            ZNetUtils.UnregisterRPC("TradeChangedClient");
            ZRoutedRpc.instance.Register("TradeChangedClient", RPC_TradeChangedClient);
            ZNetUtils.UnregisterRPC("CancelTradingClient");
            ZRoutedRpc.instance.Register("CancelTradingClient", RPC_CancelTradingClient);
        }

        public void StartTrade(Player localPlayer, Player otherPlayer)
        {
            _localPlayer = localPlayer;
            _otherPlayer = otherPlayer;
            TradeWindowManager.Instance.StartNewInstance();
            StoreReferences();
        }


        public Inventory GetToTradeInventory() => _toTrade;

        public Inventory GetToReceieveInventory() => _toReceive;

        public void OnToTradeInventoryChanged()
        {
            HasAccepted = false;
            OtherPlayerHasAccepted = false;
            SendItemData();
        }

        private void Update() 
        {
            CheckTradeDistance();
        }

        private void SendItemData()
        {
            ZPackage package = new ZPackage();
            _toTrade.Save(package);
            ZRoutedRpc.instance.InvokeRoutedRPC(_otherPlayer.GetOwner(), "SendTradeData", package);
        }

        private void CheckTradeDistance()
        {
            float distance = Vector3.Distance(_localPlayerTransform.position, _otherPlayerTransform.position);
            if (distance > _maxDistance)
                DestroyInstance();
        }

        /*
        private void CheckInputs()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                Destroy(this);
            }
        }
        */

        private void AcceptTrade()
        {
            if (!CanAcceptTrade())
            {
                MessageHud.instance.ShowBiomeFoundMsg("Not enough inventory slots", false);
                return;
            }

            HasAccepted = true;
            ZRoutedRpc.instance.InvokeRoutedRPC(_otherPlayer.GetOwner(), "AcceptTrade");
            if (OtherPlayerHasAccepted)
                FinalizeTrade();
        }

        private bool CanAcceptTrade()
        {
            return CanItemsFit();
        }

        private bool CanItemsFit()
        {
            Inventory playerInv = _localPlayer.GetInventory();
            Inventory fakeInv = new Inventory("CHECK", null, playerInv.GetWidth(), playerInv.GetHeight());

            playerInv.m_inventory.ForEach(item => fakeInv.AddItem(item.Clone()));

            foreach (ItemDrop.ItemData item in _toReceive.m_inventory)
            {
                if (fakeInv.CanAddItem(item.Clone()))
                {
                    fakeInv.AddItem(item.Clone());
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private void ChangeTrade()
        {
            HasAccepted = false;
            ZRoutedRpc.instance.InvokeRoutedRPC(_otherPlayer.GetOwner(), "TradeChangedClient");
        }

        private void FinalizeTrade()
        {
            MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft, "Trade successful");
            _localPlayer.GetInventory().MoveAll(_toReceive);
            _toTrade.RemoveAll();

            _tradeFinalized = true;
            Destroy(this);
        }

        private void NotifyCancelTrade()
        {
            MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft, "You have cancelled the trade");
            ZRoutedRpc.instance.InvokeRoutedRPC(_otherPlayer.GetOwner(), "CancelTradingClient");
        }

        private void DestroyInstance() => Destroy(this);

        private void OnDestroy()
        {
            TradeWindowManager.Instance.CancelInstance();
            InventoryGui.instance.Hide();
            UnsubscribeToGuiEvents();

            if (!_cancelledByOtherPlayer && !_tradeFinalized)
                NotifyCancelTrade();
        }

        #region RPCs

        private void RPC_AcceptTrade(long sender)
        {
            OtherPlayerHasAccepted = true;
            if (HasAccepted)
                FinalizeTrade();
        }

        private void RPC_ReceiveTradeData(long sender, ZPackage data)
        {
            HasAccepted = false;
            OtherPlayerHasAccepted = false;
            _toReceive.Load(data);
            TradeWindowManager.Instance.RefreshToReceiveWindow();
        }

        private void RPC_CancelTradingClient(long sender)
        {
            string name = ZNetUtils.GetPlayer(sender).GetPlayerName();
            MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft, name + " has cancelled the trade");

            _cancelledByOtherPlayer = true;
            Destroy(this);
        }
        
        private void RPC_TradeChangedClient(long sender)
        {
            OtherPlayerHasAccepted = false;
        }

        public void OnNewLocalPlayer()
        {
            _localPlayer = Player.m_localPlayer;
            _otherPlayerTransform = _localPlayer.transform;
        }

        #endregion

    }
}
