using System;
using PlayerTrading.Patterns;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerTrading.GUI
{
    // This singelton should not be destroyed when the mod is disabled, its passive and prevents bugs from refreshing.
    class TradeWindowManager : MonoSingleton<TradeWindowManager>
    {
        private TradeWindow _toTradeWindow;
        private TradeWindow _toReceiveWindow;

        private GameObject _acceptTradeButtonGO;
        private Button _acceptTradeButton;
        private GameObject _cancelTradeButtonGO;
        private Button _cancelTradeButton;

        private const string AcceptButtonText = "Accept Trade";
        private const string ChangeButtonText = "Change Trade";
        private const string CancelButtonText = "Cancel Trade";

        public event Action OnTradeAcceptPressed;
        public event Action OnCancelTradePressed;
        public event Action OnChangeTradePressed;
        
        public void ForceResetContainerPos() => _toTradeWindow.ResetDefaultPosition();

        public void SetToTradeAccepted(bool accepted)
        {
            _toTradeWindow.SetAsAccepted(accepted);
            if (accepted)
            {
                _acceptTradeButtonGO.GetComponentInChildren<Text>().text = ChangeButtonText;
                _acceptTradeButton.onClick = new Button.ButtonClickedEvent();
                _acceptTradeButton.onClick.AddListener(ChangeButtonClicked);
            }
            else
            {
                _acceptTradeButtonGO.GetComponentInChildren<Text>().text = AcceptButtonText;
                _acceptTradeButton.onClick = new Button.ButtonClickedEvent();
                _acceptTradeButton.onClick.AddListener(AcceptButtonClicked);
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
            _toReceiveWindow.Hide();

            _toTradeWindow = gameObject.AddComponent<ContainerTradeWindow>();
            _toTradeWindow.Initialise("You Will Give", TradeWindow.WindowPositionType.LEFT);
            _toTradeWindow.Hide();  
        }

        private void InitialiseButtons()
        {
            float refResWidth = 1920;
            float refResHeight = 1080;
            float widthMultiplier = Screen.width / refResWidth;
            float heightMultiplier = Screen.height / refResHeight;
            float guiScale = PlayerPrefs.GetFloat("GuiScale", 1f);

            GameObject buttonPrefab = InventoryGui.instance.m_takeAllButton.gameObject;

            // Accept Button
            _acceptTradeButtonGO = Instantiate(buttonPrefab);
            _acceptTradeButton = _acceptTradeButtonGO.GetComponentInChildren<Button>();
            _acceptTradeButton.transform.SetParent(InventoryGui.instance.m_inventoryRoot);
            RectTransform transform = _acceptTradeButtonGO.GetComponent<RectTransform>();

            float xOffset = 0f;
            float yOffset = (Screen.height / (30f));

            float width = ((Screen.width / 2) + xOffset);
            float height = ((Screen.height / 2) + yOffset);

            Vector2 newPos = Camera.main.ScreenToViewportPoint(new Vector3(width, height, 0f));
            transform.anchorMin = newPos;
            transform.anchorMax = newPos;
            transform.anchoredPosition = newPos;

            float newX = transform.localScale.x * widthMultiplier * guiScale;
            float newY = (newX / (16 / 9));
            transform.localScale = new Vector3(newX, newY, transform.localScale.z);


            _acceptTradeButton.name = AcceptButtonText;
            _acceptTradeButton.GetComponentInChildren<Text>().text = AcceptButtonText;
            _acceptTradeButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            _acceptTradeButton.GetComponent<Button>().onClick.AddListener(AcceptButtonClicked);
            _acceptTradeButtonGO.SetActive(false);

            // Cancel Button 
            _cancelTradeButtonGO = Instantiate(buttonPrefab);
            _cancelTradeButton = _cancelTradeButtonGO.GetComponentInChildren<Button>();
            _cancelTradeButton.transform.SetParent(InventoryGui.instance.m_inventoryRoot);

            transform = _cancelTradeButtonGO.GetComponent<RectTransform>();

            xOffset = 0f;
            yOffset = (Screen.height / 30f);

            width = ((Screen.width / 2) + xOffset);
            height = ((Screen.height / 2) - yOffset);

            newPos = Camera.main.ScreenToViewportPoint(new Vector3(width, height, 0f));
            transform.anchorMin = newPos;
            transform.anchorMax = newPos;
            transform.anchoredPosition = newPos;

            newX = transform.localScale.x * widthMultiplier * guiScale;
            newY = (newX / (16 / 9));
            transform.localScale = new Vector3(newX, newY, transform.localScale.z);

            _cancelTradeButton.name = CancelButtonText;
            _cancelTradeButton.GetComponentInChildren<Text>().text = CancelButtonText;
            _cancelTradeButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            _cancelTradeButton.GetComponent<Button>().onClick.AddListener(CancelButtonClicked);
            _cancelTradeButtonGO.SetActive(false);
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
            InventoryGui.instance.m_animator.speed = 9999f;
            _toReceiveWindow.ResetForNewInstance();
            _toReceiveWindow.Show();
            _toTradeWindow.ResetForNewInstance();
            _toTradeWindow.Show();
            _acceptTradeButtonGO.SetActive(true);
            _cancelTradeButtonGO.SetActive(true);
            HUDTools.SetHUDsActive(false);
        }

        public void CancelInstance()
        {
            _acceptTradeButtonGO.SetActive(false);
            _cancelTradeButtonGO.SetActive(false);
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
