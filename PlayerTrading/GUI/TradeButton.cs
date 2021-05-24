using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PlayerTrading.GUI
{
    class TradeButton : MonoBehaviour
    {
        private GameObject _buttonGameObject;
        private Button _button;
        private RectTransform _rectTransform;
        private UnityAction _onClick;
        private UIGroupHandler _uiHandler;
        private Text _text;
        private Image _image;
        private Color _originalColor;
        private ConfigEntry<Vector2> _userConfig;

        private string _name;
        private string _joyButton;
        private string _joyButtonHint;
        private float _xOffset;
        private float _yOffset;
        private float _userXOffset = 0f;
        private float _userYOffset = 0f;
        private const float ButtonMoveSpeed = 8f;


        private bool _inEditPositionMode;
        private bool _isMovingButton;

        private void Awake()
        {
            InitialiseButton();
        }

        private void Update()
        {
            if (_inEditPositionMode)
                UpdateEditPosition();
        }

        private void UpdateEditPosition()
        {
            Vector2 localMousePosition = _rectTransform.InverseTransformPoint(Input.mousePosition);
            if (Input.GetMouseButtonDown(0) && _rectTransform.rect.Contains(localMousePosition))
            {
                _isMovingButton = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _isMovingButton = false;
                SaveUserOffsets();
            }

            if (_isMovingButton)
            {
                _userXOffset += Input.GetAxis("Mouse X") * ButtonMoveSpeed;
                _userYOffset += Input.GetAxis("Mouse Y") * ButtonMoveSpeed;
                UpdatePosition();
            }
        }

        private void SaveUserOffsets()
        {
            _userConfig.Value = new Vector2(_userXOffset, _userYOffset);
        }

        private void InitialiseButton()
        {
            float refResWidth = 1920;
            float refResHeight = 1080;
            float widthMultiplier = Screen.width / refResWidth;
            float heightMultiplier = Screen.height / refResHeight;
            float guiScale = PlayerPrefs.GetFloat("GuiScale", 1f);

            GameObject buttonPrefab = InventoryGui.instance.m_takeAllButton.gameObject;

            _buttonGameObject = Instantiate(buttonPrefab);
            _button = _buttonGameObject.GetComponentInChildren<Button>();
            _button.transform.SetParent(InventoryGui.instance.m_inventoryRoot);
            _rectTransform = _buttonGameObject.GetComponent<RectTransform>();

            UpdatePosition();

            float newX = _rectTransform.localScale.x * widthMultiplier * guiScale;
            float newY = (newX / (16 / 9));
            _rectTransform.localScale = new Vector3(newX, newY, transform.localScale.z);

            _button.name = _name;
            _text = _button.GetComponentInChildren<Text>();
            _text.text = _name;
            _button.onClick = new Button.ButtonClickedEvent();
            _button.onClick.AddListener(_onClick);
            _buttonGameObject.SetActive(false);

            UIGamePad uiGamepad = _buttonGameObject.GetComponent<UIGamePad>();
            uiGamepad.m_zinputKey = _joyButton;
            uiGamepad.m_group = _uiHandler;
            _buttonGameObject.transform.Find("gamepad_hint").GetComponentInChildren<Text>().text = _joyButtonHint;
            _image = _buttonGameObject.GetComponent<Image>();
            _originalColor = _image.color;

        }

        private void UpdatePosition()
        {
            float width = ((Screen.width / 2) + _xOffset + _userXOffset);
            float height = ((Screen.height / 2) + _yOffset + _userYOffset);

            Vector2 newPos = Camera.main.ScreenToViewportPoint(new Vector3(width, height, 0f));
            _rectTransform.anchorMin = newPos;
            _rectTransform.anchorMax = newPos;
            _rectTransform.anchoredPosition = newPos;
        }

        public void Init(string name, UnityAction onClick, ConfigEntry<Vector2> userConfig, UIGroupHandler uiGroupHandler, string joyButton, string joyButtonHint, float xOffset, float yOffset)
        {
            _name = name;
            _joyButton = joyButton;
            _joyButtonHint = joyButtonHint;
            _xOffset = xOffset;
            _yOffset = yOffset;
            _onClick = onClick;
            _uiHandler = uiGroupHandler;
            _userConfig = userConfig;
            _userXOffset = _userConfig.Value.x;
            _userYOffset = _userConfig.Value.y;
            InitialiseButton();
        }

        public void SetEditPosMode(bool modeOn)
        {
            _inEditPositionMode = modeOn;
            if (_inEditPositionMode)
            {
                _image.color = Color.Lerp(_originalColor, Color.magenta, 0.35f);
            }
            else
            {
                _image.color = _originalColor;
            }
        }

        public void SetText(string text)
        {
            _button.GetComponentInChildren<Text>().text = text;
        }

        public void SetOnClickAction(UnityAction action)
        {
            _onClick = action;
            _button.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            _button.GetComponent<Button>().onClick.AddListener(_onClick);
        }

        public void SetActive(bool active) => _buttonGameObject.SetActive(active);

       
    }
}
