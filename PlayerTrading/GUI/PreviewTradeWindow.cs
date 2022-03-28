using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerTrading.GUI
{
    class PreviewTradeWindow : TradeWindow
    {
        private GameObject? _containerHUD;
        private InventoryGrid? _grid;
        private Transform? _gridRoot;

        private const int WindowWidth = 6;
        private const int WindowHeight = 4;
        private Text? _inventoryWeight;
        private bool _isMovingWindow;

        public override void Initialise(string tradeWindowName, WindowPositionType windowPosition)
        {
            Name = tradeWindowName;
            WindowPosition = windowPosition;
            SetupWindow();
            StoreReferences();
            SetInitialWeight();
            UpdatePosition();
            Initialised = true;
        }

        private void Update()
        {
            if (WindowEditMode)
                UpdateWindowEditMode();
        }

        private void UpdateWindowEditMode()
        {
            Vector2 localMousePosition = TradeWindowGUIRT!.InverseTransformPoint(Input.mousePosition);
            if (Input.GetMouseButtonDown(0) && TradeWindowGUIRT.rect.Contains(localMousePosition))
            {
                _isMovingWindow = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _isMovingWindow = false;
                SaveUserOffsets();
            }

            if (_isMovingWindow)
                AddUserOffsets(Input.GetAxis("Mouse X") * WindowEditMoveSpeed, -Input.GetAxis("Mouse Y") * WindowEditMoveSpeed);

        }

        private void StoreReferences()
        {
            LocalPlayer = Player.m_localPlayer;
            TradeWindowGUIRT = _containerHUD!.GetComponent<RectTransform>();
            WindowBackground = TradeWindowGUIRT.transform.Find("Bkg").GetComponent<Image>();
            OriginalBkgColor = WindowBackground.color;
        }

        private void SaveUserOffsets()
        {
            PlayerTradingMain.ToReceiveUserOffset!.Value = GetUserOffsets();
        }

        private void SetupWindow()
        {
            WindowInventory = new Inventory(Name, null, WindowWidth, WindowHeight);
            GameObject containerPrefab = InventoryGui.m_instance.m_container.gameObject;
            _containerHUD = Instantiate(containerPrefab, containerPrefab.transform.parent);

            _grid = _containerHUD.GetComponentInChildren<InventoryGrid>();
            ResetGrid(_grid);

            _containerHUD.SetActive(true);
            _containerHUD.transform.Find("container_name").GetComponent<Text>().text = Name;
            _containerHUD.transform.Find("TakeAll").gameObject.SetActive(false);

            UIGroupHandler uiGhandler = _containerHUD.GetComponent<UIGroupHandler>();
            uiGhandler.ResetActiveElement();
            uiGhandler.SetActive(false);

            WindowInventory.UpdateTotalWeight();
        }

        private void SetInitialWeight()
        {
            _inventoryWeight = TradeWindowGUIRT!.Find("Weight").GetComponentInChildren<Text>();
            _inventoryWeight.text = ((int)Math.Ceiling(WindowInventory!.GetTotalWeight())).ToString();
        }

        private void ResetGrid(InventoryGrid grid)
        {
            if (!_gridRoot)
                _gridRoot = grid.gameObject.transform.Find("Root");

            for (int i = 0; i < _gridRoot!.childCount; i++)
            {
                Destroy(_gridRoot.GetChild(i).gameObject);
            }

            grid.m_elements.Clear();
            grid.m_height = 0;
            grid.m_width = 0;
            grid.m_inventory = WindowInventory;
            grid.UpdateInventory(WindowInventory, null, null);
            WindowInventory!.Changed();
        }

        public override bool IsShowing()
        {
            return TradeWindowGUIRT!.gameObject.activeInHierarchy;
        }

        public override void DestroyTradeWindow()
        {
            if (_containerHUD!.gameObject)
                Destroy(_containerHUD.gameObject);
            WindowInventory = null!;
            TradeWindowGUIRT = null!;
        }

        public override void Show()
        {
            UpdatePosition();
            if (_containerHUD)
                _containerHUD!.SetActive(true);
        }

        public override void SetAsAccepted(bool accepted)
        {
            if (accepted)
                WindowBackground!.color = Color.Lerp(OriginalBkgColor, Color.green, 0.35f);
            else
                WindowBackground!.color = OriginalBkgColor;
        }

        public override List<ItemDrop.ItemData> GetItems()
        {
            return WindowInventory!.GetAllItems();
        }

        public override void Hide()
        {
            if (_containerHUD)
                _containerHUD!.SetActive(false);
        }

        public override void OnTradeCancelled()
        {
            Hide();
            WindowInventory!.RemoveAll();
        }

        public override void ResetForNewInstance()
        {
            WindowInventory?.RemoveAll();
            ResetGrid(_grid!);
        }

        public override void Refresh()
        {
            WindowInventory?.UpdateTotalWeight();
            ResetGrid(_grid!);
            _inventoryWeight!.text = ((int)Math.Ceiling(WindowInventory!.GetTotalWeight())).ToString();
        }

        public override void ResetDefaultPosition()
        {
            ResetPosition();
        }

        public override UIGroupHandler GetUIGroupHandler()
        {
            return _containerHUD!.GetComponent<UIGroupHandler>();
        }

        public override void SetAsWindowEditMode(bool modeOn)
        {
            WindowEditMode = modeOn;

            if (modeOn)
            {
                WindowBackground!.color = Color.Lerp(OriginalBkgColor, Color.magenta, 0.35f);
            }
            else
            {
                WindowBackground!.color = OriginalBkgColor;
                SaveUserOffsets();
            }      
        }
    }
}
