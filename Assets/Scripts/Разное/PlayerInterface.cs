using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerInterface : MonoBehaviour
{
    [Header("���������")]
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

    [SerializeField] private GameObject[] _shipInterfaceGameObjects;
    [SerializeField] private Image[] _shipInterfaceImages;
    [SerializeField] private bool[] _shipInterfaceImagesToEnable;
    [SerializeField] private float _shipInterfaceAlphaChangingSpeed = 5f;

    [Header("�������")]
    [SerializeField] private bool _shipInterfaceEnabled;
    [SerializeField] private List<TranslatedText> _warningMessagesList;
    public Player LocalPlayer;

    public static PlayerInterface Instance;
    private bool _shipInterfaceGameObjectsEnabled;
    private float _shipInterfaceAlpha;

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

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("�� ����� ��������� PlayerInterface, ���� ���� �� ������");
        }
        else
        {
            Instance = this;
        }
        foreach (AttackButton attackButton in _attackButtons)
        {
            attackButton.pointerStateChangedMessage += SendAttackButtonStateChangedMessage;
        }
    }

    private void OnDestroy()
    {
        foreach (AttackButton attackButton in _attackButtons)
        {
            attackButton.pointerStateChangedMessage -= SendAttackButtonStateChangedMessage;
        }
    }

    private void Start()
    {
        _shipInterfaceElementsAlphas = new float[_shipInterfaceImages.Length];
        if (!_shipInterfaceEnabled)
        {
            foreach (GameObject intefaceElement in _shipInterfaceGameObjects)
            {
                intefaceElement.SetActive(false);
            }
            _shipInterfaceGameObjectsEnabled = false;
            foreach (Image intefaceElement in _shipInterfaceImages)
            {
                Color oldColor = intefaceElement.color;
                intefaceElement.color = new Color(oldColor.r, oldColor.g, oldColor.b, 0);
            }
            _shipInterfaceAlpha = 0;
        }
    }

    void Update()
    {
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
            SetActivePlayerInterface(false);
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

    public void SetActivePlayerInterface(bool state)
    {
        _shipInterfaceEnabled = state;
        if (state == false)
        {
            _movementJoystickIconAlphaGrowingUp = true;
            _movementJoystickIconSleeping = true;
            _movementJoystickIconTimerSleep = 0;
            _movementJoystickIconAlpha = 0;
            _movementJoystickHandleLightAlpha = 0;

            for (int intefaceElementNum = 0; intefaceElementNum < _shipInterfaceImages.Length; intefaceElementNum++)
            {
                _shipInterfaceElementsAlphas[intefaceElementNum] = _shipInterfaceImages[intefaceElementNum].color.a;
            }
        }
    }

    void UpdatePlayerInterfaceVisualState()
    {
        if (_shipInterfaceEnabled)
        {
            if (!_shipInterfaceGameObjectsEnabled)
            {
                _shipInterfaceAlpha = 0;
                foreach (GameObject intefaceElement in _shipInterfaceGameObjects)
                {
                    intefaceElement.SetActive(true);
                }
                _shipInterfaceGameObjectsEnabled = true;
            }
            if (_shipInterfaceAlpha < 1)
            {
                _shipInterfaceAlpha += _shipInterfaceAlphaChangingSpeed * Time.deltaTime;
                if (_shipInterfaceAlpha > 1)
                {
                    _shipInterfaceAlpha = 1;
                }
                for (int intefaceElementNum = 0; intefaceElementNum < _shipInterfaceImages.Length; intefaceElementNum++)
                {
                    if (_shipInterfaceImagesToEnable[intefaceElementNum])
                    {
                        Color oldColor = _shipInterfaceImages[intefaceElementNum].color;
                        _shipInterfaceImages[intefaceElementNum].color = new Color(oldColor.r, oldColor.g, oldColor.b, _shipInterfaceAlpha);
                    }
                }
            }
        }
        else
        {
            if (_shipInterfaceAlpha > 0)
            {
                _shipInterfaceAlpha -= _shipInterfaceAlphaChangingSpeed * Time.deltaTime;
                if (_shipInterfaceAlpha < 0)
                {
                    _shipInterfaceAlpha = 0;
                    foreach (GameObject intefaceElement in _shipInterfaceGameObjects)
                    {
                        intefaceElement.SetActive(false);
                    }
                    _shipInterfaceGameObjectsEnabled = false;
                }
                for (int intefaceElementNum = 0; intefaceElementNum < _shipInterfaceImages.Length; intefaceElementNum++)
                {
                    Color oldColor = _shipInterfaceImages[intefaceElementNum].color;
                    _shipInterfaceImages[intefaceElementNum].color = new Color(oldColor.r, oldColor.g, oldColor.b, _shipInterfaceAlpha * _shipInterfaceElementsAlphas[intefaceElementNum]);
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
}
