using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerTrading.GUI
{
    class ContainerTradeWindow : TradeWindow
    {
        private const int WindowWidth = 6;
        private const int WindowHeight = 4;
        private RectTransform? _takeAllButtonTransform;
        private RectTransform? _gridRoot;
        private InventoryGrid? _grid;
        private bool _flipped;
        private bool _isMovingWindow;

        public override void Initialise(string tradeWindowName, WindowPositionType windowPosition)
        {
            Name = tradeWindowName;
            WindowPosition = windowPosition;
            StoreReferences();
            SetUpLocalInventory();
            SetDefaultPosition();
            UpdatePosition();
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
                AddUserOffsets(-Input.GetAxis("Mouse X") * WindowEditMoveSpeed, -Input.GetAxis("Mouse Y") * WindowEditMoveSpeed);

        }

        private void LateUpdate()
        {
            if (WindowActive)
                UpdatePosition();
        }

        private void StoreReferences()
        {
            LocalPlayer = Player.m_localPlayer;
            TradeWindowGUIRT = InventoryGui.m_instance.m_container;
            _grid = TradeWindowGUIRT.GetComponentInChildren<InventoryGrid>();
            WindowBackground = TradeWindowGUIRT.transform.Find("Bkg").GetComponent<Image>();
            OriginalBkgColor = WindowBackground.color;
            _takeAllButtonTransform = TradeWindowGUIRT.Find("TakeAll").GetComponent<RectTransform>();
        }

        private void SaveUserOffsets()
        {
            PlayerTradingMain.ToGiveUserOffset!.Value = GetUserOffsets();
        }

        private void SetUpLocalInventory()
        {
            WindowInventory = new Inventory(Name, null, WindowWidth, WindowHeight);
        }

        public override List<ItemDrop.ItemData> GetItems()
        {
            return WindowInventory!.GetAllItems();
        }

        private void ResetGrid(InventoryGrid grid)
        {
            if (!_gridRoot)
                _gridRoot = grid.gameObject.transform.Find("Root").GetComponent<RectTransform>();

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


        public override void SetAsAccepted(bool accepted)
        {
            if (accepted)
                WindowBackground!.color = Color.Lerp(OriginalBkgColor, Color.green, 0.35f);
            else
                WindowBackground!.color = OriginalBkgColor;
        }

        public override void Show()
        {
            if (!_flipped)
            {
                FlipHUD();
                _flipped = true;
            }

            InventoryGui.m_instance.m_containerName.text = Name;
            UpdatePosition();
            WindowActive = true;
            InventoryGui.instance.Show(null);
        }

        public override void Hide()
        {
            if (_flipped)
            {
                FlipHUD();
                _flipped = false;
            }

            ResetPosition();
            WindowActive = false;
            InventoryGui.instance.CloseContainer();
            InventoryGui.instance.Hide();
        }

        private void FlipHUD()
        {
            Vector3 pos = _takeAllButtonTransform!.anchoredPosition;
            RectTransformUtility.FlipLayoutOnAxis(TradeWindowGUIRT, 0, true, true);
            RectTransformUtility.FlipLayoutOnAxis(_gridRoot, 0, true, true);
            _takeAllButtonTransform.anchoredPosition = pos;
        }

        public override void DestroyTradeWindow()
        {
            WindowInventory = null;
            TradeWindowGUIRT = null;
            WindowActive = false;
            Initialised = false;
            InventoryGui.instance.CloseContainer();
        }

        public override void OnTradeCancelled()
        {
            ResetPosition();
            Hide();
            LocalPlayer!.GetInventory().MoveAll(WindowInventory);
        }

        public override bool IsShowing()
        {
            return InventoryGui.instance.IsContainerOpen();
        }

        public override void ResetForNewInstance()
        {
            WindowInventory?.RemoveAll();
            ResetGrid(_grid!);
        }

        public override void Refresh()
        {
            //WindowInventory.Changed();
            ResetGrid(_grid!);
        }

        public override void ResetDefaultPosition()
        {
            ResetPosition();
        }

        public override UIGroupHandler GetUIGroupHandler()
        {
            return TradeWindowGUIRT!.GetComponent<UIGroupHandler>();
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
