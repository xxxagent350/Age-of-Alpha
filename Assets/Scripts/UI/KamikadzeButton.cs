using UnityEngine;
using UnityEngine.UI;

public class KamikadzeButton : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] private float _kamikadzeModeEnablingDelayTime = 0.5f;
    [SerializeField] private UIPressedInfo _pressedInfo;
    [SerializeField] private float _buttonPressedAlpha = 0.7f;
    [SerializeField] private Image _buttonCircle;
    [SerializeField] private Image _buttonIcon;
    [SerializeField] private Image _animationCircle;
    [SerializeField] private Image _animationHighlightedCircle;
    [SerializeField] private Image _animationHighlightedIcon;
    [SerializeField] private float _buttonBlinkingTime = 0.2f;
    [SerializeField] private Color _normalButtonColor = Color.white;
    [SerializeField] private Color _warningButtonColor = Color.red;

    [Header("Отладка")]
    [SerializeField] private bool _kamikadzeModeEnabled;

    private bool _buttonPressed;
    private float _delayTimer;
    private float _buttonBlinkingTimer;

    public delegate void KamikadzeModeEnabledEventHandler();
    public event KamikadzeModeEnabledEventHandler KamikadzeModeEnabledEvent;

    private void Update()
    {
        CheckDelayTimer();
        CheckIfButtonPressed();
        AnimateButtonPressedState();
        AnimateDelay();
        AnimateButtonBlinking();
    }

    private void CheckDelayTimer()
    {
        if (_buttonPressed)
        {
            if (_delayTimer < _kamikadzeModeEnablingDelayTime)
            {
                _delayTimer += Time.deltaTime;
            }
            else
            {
                if (_kamikadzeModeEnabled == false)
                {
                    _kamikadzeModeEnabled = true;
                    KamikadzeModeEnabledEvent?.Invoke();
                }
            }
        }
        else
        {
            _delayTimer = 0;
        }
    }

    private void CheckIfButtonPressed()
    {
        _buttonPressed = _pressedInfo.publicTouchesPositions.Length > 0;
    }

    private void AnimateButtonPressedState()
    {
        float targetAlpha = _buttonPressed ? _buttonPressedAlpha : 1;
        Color oldColorButtonCircle = _buttonCircle.color;
        _buttonCircle.color = new Color(oldColorButtonCircle.r, oldColorButtonCircle.g, oldColorButtonCircle.b, targetAlpha);
        Color oldColorButtonIcon = _buttonIcon.color;
        _buttonIcon.color = new Color(oldColorButtonIcon.r, oldColorButtonIcon.g, oldColorButtonIcon.b, targetAlpha);
    }

    private void AnimateDelay()
    {
        bool enableDelayAnimation = _buttonPressed && !_kamikadzeModeEnabled;

        _animationCircle.enabled = enableDelayAnimation;
        _animationHighlightedCircle.enabled = enableDelayAnimation;
        _animationHighlightedIcon.enabled = enableDelayAnimation;

        if (enableDelayAnimation)
        {
            float animationProgress = _delayTimer / _kamikadzeModeEnablingDelayTime;

            Color oldColorAnimationHighlightedIcon = _animationHighlightedIcon.color;
            _animationHighlightedIcon.color = new Color(oldColorAnimationHighlightedIcon.r, oldColorAnimationHighlightedIcon.g, oldColorAnimationHighlightedIcon.b, animationProgress);
            _animationHighlightedCircle.fillAmount = animationProgress;
        }
    }

    private void AnimateButtonBlinking()
    {
        if (_kamikadzeModeEnabled)
        {
            _buttonBlinkingTimer += Time.deltaTime;
            if (_buttonBlinkingTimer < _buttonBlinkingTime)
            {
                _buttonCircle.color = _warningButtonColor;
                _buttonIcon.color = _normalButtonColor;
            }
            if (_buttonBlinkingTimer > _buttonBlinkingTime)
            {
                _buttonCircle.color = _normalButtonColor;
                _buttonIcon.color = _warningButtonColor;
            }
            if (_buttonBlinkingTimer >= _buttonBlinkingTime * 2)
            {
                _buttonBlinkingTimer = 0;
            }
        }
    }

    public void ResetAnimationsAndDelay()
    {
        _buttonPressed = false;
        _kamikadzeModeEnabled = false;
        _delayTimer = 0;
        _buttonBlinkingTimer = 0;

        _buttonCircle.color = _normalButtonColor;
        _buttonIcon.color = _normalButtonColor;

        _animationCircle.enabled = false;
        _animationHighlightedCircle.enabled = false;
        _animationHighlightedIcon.enabled = false;
    }
}
