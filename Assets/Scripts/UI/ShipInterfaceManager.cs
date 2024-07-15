using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System;

public class ShipInterfaceManager : MonoBehaviour
{
    [Header("Настройка")]
    public EnergyBar energyBar;

    [SerializeField] private AttackButton[] _attackButtons;

    [SerializeField] private TextMeshProUGUI _warningText;
    [SerializeField] private AudioClip _warningSound;
    [SerializeField] private float _warningSoundVolume = 0.7f;
    [SerializeField] private float _warningLabelMaxTimer = 6;
    [SerializeField] private int _warningLabelBlinkingTimes = 6;

    [SerializeField] private Joystick _movementJoystick;
    [SerializeField] private Image _movementJoystickIcon;
    [SerializeField] private float _movementJoystickIconSleepingTime = 1;
    [SerializeField] private float _movementJoystickIconAlphaChangingSpeed = 1;

    [SerializeField] private Image _movementJoystickHandleLight;
    [SerializeField] private float _movementJoystickHandleLightAlphaChangingSpeed = 2;

    [SerializeField] private Radar _radar;
    public KamikadzeButton KamikadzeButton;

    [SerializeField] private List<InterfaceElement> _interfaceElements;
    private const float ShipInterfaceAlphaChangingSpeed = 5f;

    [Header("Отладка")]
    [SerializeField] private bool _shipInterfaceEnabled;
    [SerializeField] private List<TranslatedText> _warningMessagesList;
    public Player LocalPlayer;

    public static ShipInterfaceManager Instance;

    private bool _shipInterfaceGameObjectsEnabled;
    [SerializeField] private float _shipInterfaceAlpha;

    private bool _movementJoystickIconAlphaGrowingUp;
    private bool _movementJoystickIconSleeping;
    private float _movementJoystickIconTimerSleep;
    private float _movementJoystickIconAlpha;
    private float _movementJoystickHandleLightAlpha;

    private bool _lastFrameMovementJoystickPressed;
    private float _lastFrameMovementJoystickDirInDegrees;
    private float _lastFrameMovementJoystickMagnitude;

    private float[] _shipInterfaceElementsAlphas;

    private float _warningLabelTimer;
    private int _warningLabelBlinkedTimes;
    private float _warningLabelBlinkingTimer;

    private bool interfaceOpacityIsGrowingDown;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("На сцене несколько PlayerInterface, чего быть не должно");
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
        foreach (AttackButton attackButton in _attackButtons)
        {
            attackButton.pointerStateChangedMessage += SendAttackButtonStateChangedMessage;
        }
        SetAutoDefaultOpacities();
        ResetJoystickAnimation();
        SetActiveInterfaceElements(_shipInterfaceEnabled);
    }

    public void SetActiveInterface(bool enableInterface)
    {
        _shipInterfaceEnabled = enableInterface;
        if (enableInterface)
        {
            KamikadzeButton.ResetAnimationsAndDelay();
            _radar.enabled = true;
        }
        else
        {
            _radar.ClearRegisteredObjects();
            _radar.enabled = false;
        }
    }

    void SetAutoDefaultOpacities()
    {
        foreach (InterfaceElement interfaceElement in _interfaceElements)
        {
            if (interfaceElement.DefaultOpacity < 0)
            {
                if (interfaceElement.Image != null)
                {
                    interfaceElement.DefaultOpacity = interfaceElement.Image.color.a;
                }
                if (interfaceElement.TextMeshProUI != null)
                {
                    interfaceElement.DefaultOpacity = interfaceElement.TextMeshProUI.color.a;
                }
            }
        }
    }

    void SetActiveInterfaceElements(bool state)
    {
        if (state == false)
        {
            ResetJoystickAnimation();
        }

        foreach (InterfaceElement interfaceElement in _interfaceElements)
        {
            if (interfaceElement.Image != null)
            {
                interfaceElement.Image.gameObject.SetActive(state);
            }
            if (interfaceElement.TextMeshProUI != null)
            {
                interfaceElement.TextMeshProUI.gameObject.SetActive(state);
            }
        }
        _shipInterfaceGameObjectsEnabled = state;
    }

    void ResetJoystickAnimation()
    {
        _movementJoystickIconAlphaGrowingUp = true;
        _movementJoystickIconSleeping = true;
        _movementJoystickIconTimerSleep = 0;
        _movementJoystickIconAlpha = 0;
        _movementJoystickHandleLightAlpha = 0;

        _movementJoystickIcon.color = new Color(_movementJoystickIcon.color.r, _movementJoystickIcon.color.g, _movementJoystickIcon.color.b, 0);
        _movementJoystickHandleLight.color = new Color(_movementJoystickHandleLight.color.r, _movementJoystickHandleLight.color.g, _movementJoystickHandleLight.color.b, 0);
    }

    private void OnDestroy()
    {
        foreach (AttackButton attackButton in _attackButtons)
        {
            attackButton.pointerStateChangedMessage -= SendAttackButtonStateChangedMessage;
        }
    }

    void Update()
    {
        if (_shipInterfaceEnabled && _shipInterfaceAlpha >= 1)
        {
            KamikadzeButton.enabled = true;
        }
        else
        {
            KamikadzeButton.enabled = false;
        }

        UpdatePlayerInterfaceVisualState();

        if (_shipInterfaceEnabled)
        {
            AnimateMovementJoystickIcon();
            AnimateMovementJoystickHandleLight();
            if (LocalPlayer != null)
            {
                ReceiveMovementJoystickValues();
            }
        }
    }

    private void FixedUpdate()
    {
        if (_warningMessagesList.Count > 0)
        {
            ShowWarningsFromList();
        }

        if (NetworkManager.Singleton.IsClient == false && _shipInterfaceEnabled)
        {
            SetActiveInterface(false);
        }
    }

    public void SetLocalPlayerToRadar(ShipGameStats shipGameStats)
    {
        _radar.PlayerShipGameStats = shipGameStats;
    }

    void ShowWarningsFromList()
    {
        if (_warningText.text == "")
        {
            _warningText.text = _warningMessagesList[0].GetTranslatedString();
            DataOperator.instance.PlayUISound(_warningSound, _warningSoundVolume);
        }
        else
        {
            _warningLabelTimer += Time.deltaTime;

            if (_warningLabelBlinkedTimes < _warningLabelBlinkingTimes)
            {
                _warningLabelBlinkingTimer += Time.deltaTime;

                if (_warningLabelBlinkingTimer < _warningSound.length / 2)
                {
                    _warningText.enabled = true;
                }
                else
                {
                    _warningText.enabled = false;
                }

                if (_warningLabelBlinkingTimer > _warningSound.length)
                {
                    _warningText.enabled = true;
                    DataOperator.instance.PlayUISound(_warningSound, _warningSoundVolume);
                    _warningLabelBlinkingTimer = 0;
                    _warningLabelBlinkedTimes++;
                }
            }

            if (_warningLabelTimer > _warningLabelMaxTimer)
            {
                _warningLabelTimer = 0;
                _warningLabelBlinkedTimes = 0;
                _warningText.text = "";
                _warningMessagesList.RemoveAt(0);
            }
        }
    }

    void UpdatePlayerInterfaceVisualState()
    {
        if (_shipInterfaceEnabled)
        {
            interfaceOpacityIsGrowingDown = false;
            if (!_shipInterfaceGameObjectsEnabled)
            {
                _shipInterfaceAlpha = 0;
                SetActiveInterfaceElements(true);
            }
            if (_shipInterfaceAlpha < 1)
            {
                _shipInterfaceAlpha += ShipInterfaceAlphaChangingSpeed * Time.deltaTime;
                if (_shipInterfaceAlpha > 1)
                {
                    _shipInterfaceAlpha = 1;
                }
                SetAlphaToInterfaceElements(_shipInterfaceAlpha, true);
            }
        }
        else
        {
            if (_shipInterfaceAlpha > 0)
            {
                if (interfaceOpacityIsGrowingDown == false)
                {
                    interfaceOpacityIsGrowingDown = true;
                    SetInterfaceOpacitiesBeforeStartedToGrowingDown();
                }
                _shipInterfaceAlpha -= ShipInterfaceAlphaChangingSpeed * Time.deltaTime;
                if (_shipInterfaceAlpha < 0)
                {
                    _shipInterfaceAlpha = 0;
                    SetActiveInterfaceElements(false);
                }
                SetAlphaToInterfaceElements(_shipInterfaceAlpha, false);
            }
        }
    }

    void SetInterfaceOpacitiesBeforeStartedToGrowingDown()
    {
        foreach (InterfaceElement interfaceElement in _interfaceElements)
        {
            if (interfaceElement.Image != null)
            {
                interfaceElement.opacityBeforeStartedToGrowingDown = interfaceElement.Image.color.a;
            }
            if (interfaceElement.TextMeshProUI != null)
            {
                interfaceElement.opacityBeforeStartedToGrowingDown = interfaceElement.TextMeshProUI.color.a;
            }
        }
    }

    void SetAlphaToInterfaceElements(float alpha, bool interfaceEnabling)
    {
        foreach (InterfaceElement interfaceElement in _interfaceElements)
        {
            if (interfaceEnabling)
            {
                if (interfaceElement.DontEnableOpacityOnSetActive == false)
                {
                    if (interfaceElement.Image != null)
                    {
                        Color oldColor = interfaceElement.Image.color;
                        interfaceElement.Image.color = new Color(oldColor.r, oldColor.g, oldColor.b, alpha * interfaceElement.DefaultOpacity);
                    }
                    if (interfaceElement.TextMeshProUI != null)
                    {
                        Color oldColor = interfaceElement.TextMeshProUI.color;
                        interfaceElement.TextMeshProUI.color = new Color(oldColor.r, oldColor.g, oldColor.b, alpha * interfaceElement.DefaultOpacity);
                    }
                }
            }
            else
            {
                if (interfaceElement.Image != null)
                {
                    Color oldColor = interfaceElement.Image.color;
                    interfaceElement.Image.color = new Color(oldColor.r, oldColor.g, oldColor.b, alpha * interfaceElement.opacityBeforeStartedToGrowingDown);
                }
                if (interfaceElement.TextMeshProUI != null)
                {
                    Color oldColor = interfaceElement.TextMeshProUI.color;
                    interfaceElement.TextMeshProUI.color = new Color(oldColor.r, oldColor.g, oldColor.b, alpha * interfaceElement.opacityBeforeStartedToGrowingDown);
                }
            }
        }
    }

    void AnimateMovementJoystickIcon()
    {
        if (!_movementJoystick.pressedOnJoystick || !_movementJoystickIconSleeping)
        {
            if (_movementJoystickIconSleeping)
            {
                _movementJoystickIconTimerSleep += Time.deltaTime;
                if (_movementJoystickIconTimerSleep > _movementJoystickIconSleepingTime)
                {
                    _movementJoystickIconSleeping = false;
                    _movementJoystickIconTimerSleep = 0;
                    _movementJoystickIconAlphaGrowingUp = true;
                }
            }
            else
            {
                if (_movementJoystickIconAlphaGrowingUp)
                {
                    _movementJoystickIconAlpha += _movementJoystickIconAlphaChangingSpeed * Time.deltaTime;
                    if (_movementJoystickIconAlpha > 1)
                    {
                        _movementJoystickIconAlphaGrowingUp = false;
                    }
                }
                else
                {
                    _movementJoystickIconAlpha -= _movementJoystickIconAlphaChangingSpeed * Time.deltaTime;
                    if (_movementJoystickIconAlpha < 0)
                    {
                        _movementJoystickIconSleeping = true;
                        _movementJoystickIconAlpha = 0;
                    }
                }
                Color oldColor = _movementJoystickIcon.color;
                _movementJoystickIcon.color = new Color(oldColor.r, oldColor.g, oldColor.b, _movementJoystickIconAlpha);
            }
        }
        else
        {
            _movementJoystickIconAlphaGrowingUp = true;
            _movementJoystickIconSleeping = true;
            _movementJoystickIconTimerSleep = 0;
            _movementJoystickIconAlpha = 0;
        }
    }

    void AnimateMovementJoystickHandleLight()
    {
        if (_movementJoystick.pressedOnJoystick)
        {
            if (_movementJoystickHandleLightAlpha < 1)
            {
                _movementJoystickHandleLightAlpha += _movementJoystickHandleLightAlphaChangingSpeed * Time.deltaTime;
            }
        }
        if (!_movementJoystick.pressedOnJoystick)
        {
            if (_movementJoystickHandleLightAlpha > 0)
            {
                _movementJoystickHandleLightAlpha -= _movementJoystickHandleLightAlphaChangingSpeed * Time.deltaTime;
            }
        }

        Color oldColor = _movementJoystickHandleLight.color;
        _movementJoystickHandleLight.color = new Color(oldColor.r, oldColor.g, oldColor.b, _movementJoystickHandleLightAlpha);
    }

    void ReceiveMovementJoystickValues()
    {
        Vector3 movementJoystickPressPos = new Vector3(_movementJoystick.Horizontal, _movementJoystick.Vertical, 0);

        bool pressed_ = _movementJoystick.pressedOnJoystick;
        float direction_ = DataOperator.GetVector2DirInDegrees(movementJoystickPressPos);
        float magnitude_ = movementJoystickPressPos.magnitude;

        if (_lastFrameMovementJoystickPressed != pressed_ || _lastFrameMovementJoystickDirInDegrees != direction_ || _lastFrameMovementJoystickMagnitude != magnitude_)
        {
            LocalPlayer.SendMovementJoystickInputsDataToServerRpc(pressed_, direction_, magnitude_);

            _lastFrameMovementJoystickPressed = pressed_;
            _lastFrameMovementJoystickDirInDegrees = direction_;
            _lastFrameMovementJoystickMagnitude = magnitude_;
        }
    }


    public delegate void AttackButtonStateChangedMessage(uint index, bool pressed);
    public event AttackButtonStateChangedMessage attackButtonStateChangedMessage;
    void SendAttackButtonStateChangedMessage(uint index, bool pressed)
    {
        /*
        if (attackButtonStateChangedMessage != null)
        {
            attackButtonStateChangedMessage(index, pressed);
        }
        */
        attackButtonStateChangedMessage?.Invoke(index, pressed);
    }


    public void ShowWarningText(TranslatedText text_)
    {
        _warningMessagesList.Add(text_);
    }

    [Serializable]
    class InterfaceElement
    {
        public Image Image;
        public TextMeshProUGUI TextMeshProUI;
        [Tooltip("Не изменять прозрачность при включении интерфейса")]
        public bool DontEnableOpacityOnSetActive;
        [Tooltip("Непрозрачность объекта, которая будет установлена при включении интерфейса (менее 0 = авто)")]
        [Range(-1, 1)] public float DefaultOpacity = -1;

        public float opacityBeforeStartedToGrowingDown;
    }
}
