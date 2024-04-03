using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInterface : MonoBehaviour
{
    [Header("Настройка")]
    public EnergyBar energyBar;

    [SerializeField] AttackButton[] attackButtons;

    [SerializeField] TextMeshProUGUI warningText;
    [SerializeField] AudioClip warningSound;
    [SerializeField] float warningSoundVolume = 0.7f;
    [SerializeField] float warningLabelMaxTimer = 6;
    [SerializeField] int warningLabelBlinkingTimes = 6;

    [SerializeField] Joystick movementJoystick;
    [SerializeField] Image movementJoystickIcon;
    [SerializeField] float movementJoystickIconSleepingTime = 1;
    [SerializeField] float movementJoystickIconAlphaChangingSpeed = 1;

    [SerializeField] Image movementJoystickHandleLight;
    [SerializeField] float movementJoystickHandleLightAlphaChangingSpeed = 2;

    [SerializeField] GameObject[] playerInterfaceGameObjects;
    [SerializeField] Image[] playerInterfaceImages;
    [SerializeField] bool[] playerInterfaceImagesToEnable;
    [SerializeField] float playerInterfaceAlphaChangingSpeed = 5f;

    [Header("Отладка")]
    [SerializeField] bool playerInterfaceEnabled;
    [SerializeField] List<TranslatedText> warningMessagesList;
    public Player localPlayer;

    public static PlayerInterface instance;
    bool playerInterfaceGameObjectsEnabled;
    float playerInterfaceAlpha;

    bool movementJoystickIconAlphaGrowingUp;
    bool movementJoystickIconSleeping;
    float movementJoystickIconTimerSleep;
    float movementJoystickIconAlpha;
    float movementJoystickHandleLightAlpha;

    bool lastFrameMovementJoystickPressed;
    float lastFrameMovementJoystickDirInDegrees;
    float lastFrameMovementJoystickMagnitude;

    float[] playerInterfaceElementsAlphas;

    float warningLabelTimer;
    int warningLabelBlinkedTimes;
    float warningLabelBlinkingTimer;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("На сцене несколько PlayerInterface, чего быть не должно");
        }
        else
        {
            instance = this;
        }
        foreach (AttackButton attackButton in attackButtons)
        {
            attackButton.pointerStateChangedMessage += SendAttackButtonStateChangedMessage;
        }
    }

    private void OnDestroy()
    {
        foreach (AttackButton attackButton in attackButtons)
        {
            attackButton.pointerStateChangedMessage -= SendAttackButtonStateChangedMessage;
        }
    }

    private void Start()
    {
        playerInterfaceElementsAlphas = new float[playerInterfaceImages.Length];
        if (!playerInterfaceEnabled)
        {
            foreach (GameObject intefaceElement in playerInterfaceGameObjects)
            {
                intefaceElement.SetActive(false);
            }
            playerInterfaceGameObjectsEnabled = false;
            foreach (Image intefaceElement in playerInterfaceImages)
            {
                Color oldColor = intefaceElement.color;
                intefaceElement.color = new Color(oldColor.r, oldColor.g, oldColor.b, 0);
            }
            playerInterfaceAlpha = 0;
        }
    }

    void Update()
    {
        UpdatePlayerInterfaceVisualState();

        if (playerInterfaceEnabled)
        {
            AnimateMovementJoystickIcon();
            AnimateMovementJoystickHandleLight();
            if (localPlayer != null)
            {
                ReceiveMovementJoystickValues();
            }
        }
    }

    private void FixedUpdate()
    {
        if (warningMessagesList.Count > 0)
        {
            ShowWarningsFromList();
        }
    }

    void ShowWarningsFromList()
    {
        if (warningText.text == "")
        {
            warningText.text = warningMessagesList[0].GetTranslatedString();
            DataOperator.instance.PlayUISound(warningSound, warningSoundVolume);
        }
        else
        {
            warningLabelTimer += Time.deltaTime;

            if (warningLabelBlinkedTimes < warningLabelBlinkingTimes)
            {
                warningLabelBlinkingTimer += Time.deltaTime;

                if (warningLabelBlinkingTimer < warningSound.length / 2)
                {
                    warningText.enabled = true;
                }
                else
                {
                    warningText.enabled = false;
                }

                if (warningLabelBlinkingTimer > warningSound.length)
                {
                    warningText.enabled = true;
                    DataOperator.instance.PlayUISound(warningSound, warningSoundVolume);
                    warningLabelBlinkingTimer = 0;
                    warningLabelBlinkedTimes++;
                }
            }

            if (warningLabelTimer > warningLabelMaxTimer)
            {
                warningLabelTimer = 0;
                warningLabelBlinkedTimes = 0;
                warningText.text = "";
                warningMessagesList.RemoveAt(0);
            }
        }
    }

    public void SetActivePlayerInterface(bool state)
    {
        playerInterfaceEnabled = state;
        if (state == false)
        {
            movementJoystickIconAlphaGrowingUp = true;
            movementJoystickIconSleeping = true;
            movementJoystickIconTimerSleep = 0;
            movementJoystickIconAlpha = 0;
            movementJoystickHandleLightAlpha = 0;

            for (int intefaceElementNum = 0; intefaceElementNum < playerInterfaceImages.Length; intefaceElementNum++)
            {
                playerInterfaceElementsAlphas[intefaceElementNum] = playerInterfaceImages[intefaceElementNum].color.a;
            }
        }
    }

    void UpdatePlayerInterfaceVisualState()
    {
        if (playerInterfaceEnabled)
        {
            if (!playerInterfaceGameObjectsEnabled)
            {
                playerInterfaceAlpha = 0;
                foreach (GameObject intefaceElement in playerInterfaceGameObjects)
                {
                    intefaceElement.SetActive(true);
                }
                playerInterfaceGameObjectsEnabled = true;
            }
            if (playerInterfaceAlpha < 1)
            {
                playerInterfaceAlpha += playerInterfaceAlphaChangingSpeed * Time.deltaTime;
                if (playerInterfaceAlpha > 1)
                {
                    playerInterfaceAlpha = 1;
                }
                for (int intefaceElementNum = 0; intefaceElementNum < playerInterfaceImages.Length; intefaceElementNum++)
                {
                    if (playerInterfaceImagesToEnable[intefaceElementNum])
                    {
                        Color oldColor = playerInterfaceImages[intefaceElementNum].color;
                        playerInterfaceImages[intefaceElementNum].color = new Color(oldColor.r, oldColor.g, oldColor.b, playerInterfaceAlpha);
                    }
                }
            }
        }
        else
        {
            if (playerInterfaceAlpha > 0)
            {
                playerInterfaceAlpha -= playerInterfaceAlphaChangingSpeed * Time.deltaTime;
                if (playerInterfaceAlpha < 0)
                {
                    playerInterfaceAlpha = 0;
                    foreach (GameObject intefaceElement in playerInterfaceGameObjects)
                    {
                        intefaceElement.SetActive(false);
                    }
                    playerInterfaceGameObjectsEnabled = false;
                }
                for (int intefaceElementNum = 0; intefaceElementNum < playerInterfaceImages.Length; intefaceElementNum++)
                {
                    Color oldColor = playerInterfaceImages[intefaceElementNum].color;
                    playerInterfaceImages[intefaceElementNum].color = new Color(oldColor.r, oldColor.g, oldColor.b, playerInterfaceAlpha * playerInterfaceElementsAlphas[intefaceElementNum]);
                }
            }
        }
    }

    void AnimateMovementJoystickIcon()
    {
        if (!movementJoystick.pressedOnJoystick || !movementJoystickIconSleeping)
        {
            if (movementJoystickIconSleeping)
            {
                movementJoystickIconTimerSleep += Time.deltaTime;
                if (movementJoystickIconTimerSleep > movementJoystickIconSleepingTime)
                {
                    movementJoystickIconSleeping = false;
                    movementJoystickIconTimerSleep = 0;
                    movementJoystickIconAlphaGrowingUp = true;
                }
            }
            else
            {
                if (movementJoystickIconAlphaGrowingUp)
                {
                    movementJoystickIconAlpha += movementJoystickIconAlphaChangingSpeed * Time.deltaTime;
                    if (movementJoystickIconAlpha > 1)
                    {
                        movementJoystickIconAlphaGrowingUp = false;
                    }
                }
                else
                {
                    movementJoystickIconAlpha -= movementJoystickIconAlphaChangingSpeed * Time.deltaTime;
                    if (movementJoystickIconAlpha < 0)
                    {
                        movementJoystickIconSleeping = true;
                        movementJoystickIconAlpha = 0;
                    }
                }
                Color oldColor = movementJoystickIcon.color;
                movementJoystickIcon.color = new Color(oldColor.r, oldColor.g, oldColor.b, movementJoystickIconAlpha);
            }
        }
        else
        {
            movementJoystickIconAlphaGrowingUp = true;
            movementJoystickIconSleeping = true;
            movementJoystickIconTimerSleep = 0;
            movementJoystickIconAlpha = 0;
        }
    }

    void AnimateMovementJoystickHandleLight()
    {
        if (movementJoystick.pressedOnJoystick)
        {
            if (movementJoystickHandleLightAlpha < 1)
            {
                movementJoystickHandleLightAlpha += movementJoystickHandleLightAlphaChangingSpeed * Time.deltaTime;
            }
        }
        if (!movementJoystick.pressedOnJoystick)
        {
            if (movementJoystickHandleLightAlpha > 0)
            {
                movementJoystickHandleLightAlpha -= movementJoystickHandleLightAlphaChangingSpeed * Time.deltaTime;
            }
        }

        Color oldColor = movementJoystickHandleLight.color;
        movementJoystickHandleLight.color = new Color(oldColor.r, oldColor.g, oldColor.b, movementJoystickHandleLightAlpha);
    }

    void ReceiveMovementJoystickValues()
    {
        Vector3 movementJoystickPressPos = new Vector3(movementJoystick.Horizontal, movementJoystick.Vertical, 0);

        bool pressed_ = movementJoystick.pressedOnJoystick;
        float direction_ = DataOperator.GetVector2DirInDegrees(movementJoystickPressPos);
        float magnitude_ = movementJoystickPressPos.magnitude;

        if (lastFrameMovementJoystickPressed != pressed_ || lastFrameMovementJoystickDirInDegrees != direction_ || lastFrameMovementJoystickMagnitude != magnitude_)
        {
            localPlayer.SendMovementJoystickInputsDataToServerRpc(pressed_, direction_, magnitude_);

            lastFrameMovementJoystickPressed = pressed_;
            lastFrameMovementJoystickDirInDegrees = direction_;
            lastFrameMovementJoystickMagnitude = magnitude_;
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
        warningMessagesList.Add(text_);
    }
}
