using System.Collections;
using System.Collections.Generic;
using PlayerTrading.GUI;
using PlayerTrading.Patterns;
using UnityEngine;

namespace PlayerTrading
{
    class TradeHandler : MonoSingleton<TradeHandler>
    {
        private Player _localPlayer;
        private HashSet<Player> _tradeRequestsSent = new HashSet<Player>();
        private HashSet<Player> _tradeRequestsReceived = new HashSet<Player>();
        private TradeInstance _currentTradeInstance;

        private bool _tradeManagerInitialised;
        private const float TradeRequestDuration = 10.0f;
        private const float MaxTradeDistance = 5.0f;

        protected override void Init() 
        {
            RegisterRPCs();
            SubscribeToEvents();
        }

        private void RegisterRPCs()
        {
            ZNetUtils.UnregisterRPC("TradeRequestedClient");
            ZNetUtils.UnregisterRPC("StartTradingClient");
            ZRoutedRpc.instance.Register<long>("TradeRequestedClient", RPC_ReceiveTradeRequestClient);
            ZRoutedRpc.instance.Register<long>("StartTradingClient", RPC_StartTradingClient);
        }

        private void SubscribeToEvents()
        {
            PlayerTradingMain.OnLocalPlayerChanged += OnNewLocalPlayer;
        }

        private void UnsubscribeToEvents()
        {
            PlayerTradingMain.OnLocalPlayerChanged -= OnNewLocalPlayer;
        }

        private void Update() 
        {
            if (!_tradeManagerInitialised)
            {
                TryInitialiseManager();
                return;
            }

            CheckInputs();
        }

        private void TryInitialiseManager()
        {
            if (!Player.m_localPlayer)
                return;

            _localPlayer = Player.m_localPlayer;
            gameObject.AddComponent<TradeWindowManager>();
            _tradeManagerInitialised = true;
        }

        private void CheckInputs()
        {
            if (ZInput.GetButtonDown("Use") || ZInput.GetButtonDown("JoyUse"))
                TrySendTradeRequest();
        }

        public bool HasTradeInstance() => _currentTradeInstance != null;

        public float GetMaxDistance() => MaxTradeDistance;

        public Inventory TryGetToTradeInventory()
        {
            if (_currentTradeInstance)
            {
                return _currentTradeInstance.GetToTradeInventory();
            }
            else
            {
                return null;
            }
        }

        public void NotifyInventoryChanged()
        {
            if (_currentTradeInstance)
                _currentTradeInstance.OnToTradeInventoryChanged();
        }

        public string GetAction(Player targetPlayer)
        {
            return (_tradeRequestsReceived.Contains(targetPlayer)) ? "Accept Trade" : "Request Trade";
        }

        public void TryCancelTradeInstance()
        {
            if (_currentTradeInstance)
            {
                Destroy(_currentTradeInstance);
            }
        }

        private void TrySendTradeRequest()
        {
            Character characterHover = _localPlayer.m_hoveringCreature;
            Player targetPlayer = null;
            if (characterHover && characterHover is Player)
                targetPlayer = characterHover as Player;

            if (targetPlayer == null || targetPlayer == Player.m_localPlayer)
                return;

            float distance = Vector3.Distance(_localPlayer.transform.position, targetPlayer.transform.position);
            if (distance > MaxTradeDistance)
                return;

            if (_tradeRequestsSent.Contains(targetPlayer))
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft, "Trade request recently sent");
                return;
            }

            if (_tradeRequestsReceived.Contains(targetPlayer))
            {
                _tradeRequestsReceived.Remove(targetPlayer);
                AcceptTradeRequest(targetPlayer);
                return;
            }

            SendTradeRequest(targetPlayer);
        }
        
        private void SendTradeRequest(Player targetPlayer)
        {
            long targetPlayerUid = ZNetUtils.GetUID(targetPlayer);

            _tradeRequestsSent.Add(targetPlayer);
            StartCoroutine(Co_ExpireTradeRequestSent(targetPlayer));

            MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft, "Trade request sent");
            ZRoutedRpc.instance.InvokeRoutedRPC(targetPlayer.GetOwner(), "TradeRequestedClient", _localPlayer.GetOwner());
        }

        private IEnumerator Co_ExpireTradeRequestSent(Player player)
        {
            yield return new WaitForSeconds(TradeRequestDuration);
            if (_tradeRequestsSent.Contains(player))
                _tradeRequestsSent.Remove(player);
        }

        private void AcceptTradeRequest(Player targetPlayer)
        {
            long targetUid = targetPlayer.GetOwner();
            RPC_StartTradingClient(0, targetUid);
            ZRoutedRpc.instance.InvokeRoutedRPC(targetUid, "StartTradingClient", _localPlayer.GetOwner());
        }

        private IEnumerator Co_ExpireTradeRequestReceived(Player player)
        {
            yield return new WaitForSeconds(TradeRequestDuration);
            if (_tradeRequestsReceived.Contains(player))
                _tradeRequestsReceived.Remove(player);
        }

        private void StartNewTradeInstance(Player otherPlayer)
        {
            if (_currentTradeInstance == null)
            {
                _currentTradeInstance = gameObject.AddComponent<TradeInstance>();
                _currentTradeInstance.StartTrade(_localPlayer, otherPlayer);
            }
            else
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft, "You cannot start a new trade session");
            }
        }

        private void OnDestroy()
        {
            if (_currentTradeInstance)
                Destroy(_currentTradeInstance);
            UnsubscribeToEvents();
        }


        #region RPCs

        private void RPC_StartTradingClient(long sender, long otherPlayerUid)
        {
            Player otherPlayer = ZNetUtils.GetPlayer(otherPlayerUid);

            if (_tradeRequestsSent.Contains(otherPlayer))
                _tradeRequestsSent.Remove(otherPlayer);

            if (_tradeRequestsReceived.Contains(otherPlayer))
                _tradeRequestsReceived.Remove(otherPlayer);

            MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft, "Started trading with " + otherPlayer.GetPlayerName());
            StartNewTradeInstance(otherPlayer);
        }

        private void RPC_ReceiveTradeRequestClient(long sender, long requesterUid)
        {
            Player requester = ZNetUtils.GetPlayer(requesterUid);
            string name = requester.GetPlayerName();

            if (requester == null)
                Debug.Log("Trade requester can't be resolved");

            if (_tradeRequestsReceived.Contains(requester))
                return;

            _tradeRequestsReceived.Add(requester);
            StartCoroutine(Co_ExpireTradeRequestReceived(requester));

            MessageHud.instance.ShowBiomeFoundMsg(name + " wants to trade", false);
        }

        public void OnNewLocalPlayer()
        {
            _localPlayer = Player.m_localPlayer;
        }

        #endregion
    }
}
