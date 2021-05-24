using System;
using PlayerTrading.Patterns;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerTrading.GUI
{
    // This singelton should not be destroyed when the mod is disabled, its passive and prevents bugs from refreshing.
    class TradeWindowManager : MonoSingleton<TradeWindowManager>
    {
        private enum TradeWindowMode
        {
            EDIT_WINDOW_ONLY,
            TRADE_INSTANCE,
            NONE
        }

        private TradeWindowMode _windowMode = TradeWindowMode.NONE;
        private TradeWindow _toTradeWindow;
        private TradeWindow _toReceiveWindow;

        private TradeButton _acceptTradeButton;
        private TradeButton _cancelTradeButton;

        private const string AcceptButtonText = "Accept Trade";
        private const string ChangeButtonText = "Change Trade";
        private const string CancelButtonText = "Cancel Trade";

        public event Action OnTradeAcceptPressed;
        public event Action OnCancelTradePressed;
        public event Action OnChangeTradePressed;

        private bool _editWindowPositionMode;
        
        public void ForceResetContainerPos() => _toTradeWindow.ResetDefaultPosition();

        public void ToggleWindowPositionMode()
        {
            _editWindowPositionMode = !_editWindowPositionMode;
            if (_editWindowPositionMode)
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Edit UI Mode ON");

                _toTradeWindow.SetAsWindowEditMode(true);
                _toReceiveWindow.SetAsWindowEditMode(true);
                _acceptTradeButton.SetEditPosMode(true);
                _cancelTradeButton.SetEditPosMode(true);

                if (_windowMode == TradeWindowMode.NONE)
                {
                    StartEditModeInstance();
                }
            }
            else
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Edit UI Mode OFF");

                _toTradeWindow.SetAsWindowEditMode(false);
                _toReceiveWindow.SetAsWindowEditMode(false);
                _acceptTradeButton.SetEditPosMode(false);
                _cancelTradeButton.SetEditPosMode(false);

                if (_windowMode == TradeWindowMode.EDIT_WINDOW_ONLY)
                {
                    CancelInstance();
                }
            }
        }

        public void DisableWindowPositionMode()
        {
            if (_editWindowPositionMode)
                ToggleWindowPositionMode();
        }

        public bool IsInWindowPositionMode() => _editWindowPositionMode;

        public void SetToTradeAccepted(bool accepted)
        {
            _toTradeWindow.SetAsAccepted(accepted);
            if (accepted)
            {
                _acceptTradeButton.SetText(ChangeButtonText);
                _acceptTradeButton.SetOnClickAction(ChangeButtonClicked);
            }
            else
            {
                _acceptTradeButton.SetText(AcceptButtonText);
                _acceptTradeButton.SetOnClickAction(AcceptButtonClicked);
            }       
        }

        public void SetToReceiveAccepted(bool accepted)
        {
            _toReceiveWindow.SetAsAccepted(accepted);
        }

        protected override void Init()
        {
            InitialiseWindows();
            InitialiseButtons();
        }

        private void InitialiseWindows()
        {
            _toReceiveWindow = gameObject.AddComponent<PreviewTradeWindow>();
            _toReceiveWindow.Initialise("You Will Receive", TradeWindow.WindowPositionType.RIGHT);
            _toReceiveWindow.SetUserOffsets(PlayerTradingMain.ToReceiveUserOffset.Value.x, PlayerTradingMain.ToReceiveUserOffset.Value.y);
            _toReceiveWindow.Hide();

            _toTradeWindow = gameObject.AddComponent<ContainerTradeWindow>();
            _toTradeWindow.Initialise("You Will Give", TradeWindow.WindowPositionType.LEFT);
            _toTradeWindow.SetUserOffsets(PlayerTradingMain.ToGiveUserOffset.Value.x, PlayerTradingMain.ToGiveUserOffset.Value.y);
            _toTradeWindow.Hide();  

        }

        private void InitialiseButtons()
        {
            _acceptTradeButton = gameObject.AddComponent<TradeButton>();
            _acceptTradeButton.Init("Accept Trade", AcceptButtonClicked, PlayerTradingMain.AcceptButtonUserOffset, _toTradeWindow.GetUIGroupHandler(), "JoyButtonX", "X", 0f, (Screen.height / (30f)));
            _cancelTradeButton = gameObject.AddComponent<TradeButton>();
            _cancelTradeButton.Init("Cancel Trade", CancelButtonClicked, PlayerTradingMain.CancelButtonUserOffset, _toTradeWindow.GetUIGroupHandler(), "JoyButtonB", "B", 0f, -(Screen.height / (30f)));
        }

        private void AcceptButtonClicked()
        {
            OnTradeAcceptPressed?.Invoke(); 
        }

        private void CancelButtonClicked()
        {
            OnCancelTradePressed?.Invoke();
        }

        private void ChangeButtonClicked()
        {
            OnChangeTradePressed?.Invoke();
        }

        public void StartNewInstance()
        {
            _windowMode = TradeWindowMode.TRADE_INSTANCE;
            InventoryGui.instance.m_animator.speed = 9999f;
            _toReceiveWindow.ResetForNewInstance();
            _toReceiveWindow.Show();
            _toTradeWindow.ResetForNewInstance();
            _toTradeWindow.Show();
            _acceptTradeButton.SetActive(true);
            _cancelTradeButton.SetActive(true);
            HUDTools.SetHUDsActive(false);
        }

        private void StartEditModeInstance()
        {
            StartNewInstance();
            _windowMode = TradeWindowMode.EDIT_WINDOW_ONLY;
        }

        public void CancelInstance()
        {
            _windowMode = TradeWindowMode.NONE;
            _acceptTradeButton.SetActive(false);
            _cancelTradeButton.SetActive(false);
            _toReceiveWindow.OnTradeCancelled();
            _toTradeWindow.OnTradeCancelled();
            SetToTradeAccepted(false);
            SetToReceiveAccepted(false);
        }

        public Inventory GetToTradeInventory() => _toTradeWindow.GetInventory();

        public Inventory GetToReceiveInventory() => _toReceiveWindow.GetInventory();

        public void RefreshToReceiveWindow()
        {
            _toReceiveWindow.Refresh();
        }
    }
}
