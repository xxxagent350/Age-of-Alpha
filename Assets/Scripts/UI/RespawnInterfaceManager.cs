using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RespawnInterfaceManager : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] private List<GameObject> _interfaceGameObjects;
    [SerializeField] private List<Image> _interfaceImages;
    [SerializeField] private List<TextMeshProUGUI> _interfaceTexts;
    [SerializeField] private TextMeshProUGUI _respawnText;

    [Header("Отладка")]
    [SerializeField] private bool _interfaceEnabled;
    [SerializeField] private float _interfaceAlpha;

    private bool _interfaceCurrentState;

    private const float _interfaceAlphaChangingSpeed = 5f;

    private void Start()
    {
        foreach (var interfaceGameObject in _interfaceGameObjects)
        {
            interfaceGameObject.SetActive(_interfaceEnabled);
        }
        if (_interfaceEnabled)
        {
            _interfaceAlpha = 1;
        }
        else
        {
            _interfaceAlpha = 0;
        }
        _interfaceCurrentState = _interfaceEnabled;
    }

    public void SetActiveInterface(bool enable)
    {
        _interfaceEnabled = enable;
        if (enable)
        {
            gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        UpdatePlayerInterfaceVisualState();
        if (_interfaceEnabled)
        {
            if (_interfaceAlpha < 1)
            {
                _interfaceCurrentState = false;
            }
        }
        else
        {
            if (_interfaceAlpha > 0)
            {
                _interfaceCurrentState = true;
            }
        }
    }

    private void UpdatePlayerInterfaceVisualState()
    {
        if (_interfaceEnabled)
        {
            if (_interfaceCurrentState == false)
            {
                _interfaceAlpha += _interfaceAlphaChangingSpeed * Time.deltaTime;
                if (_interfaceAlpha > 1)
                {
                    _interfaceAlpha = 1;
                    _interfaceCurrentState = true;
                }
                foreach (var interfaceImage in _interfaceImages)
                {
                    ChangeAlpha(interfaceImage, _interfaceAlpha);
                }
                foreach (var interfaceText in _interfaceTexts)
                {
                    ChangeAlpha(interfaceText, _interfaceAlpha);
                }
                foreach (var interfaceGameObject in _interfaceGameObjects)
                {
                    interfaceGameObject.SetActive(true);
                }
            }
        }
        else
        {
            if (_interfaceCurrentState == true)
            {
                _interfaceAlpha -= _interfaceAlphaChangingSpeed * Time.deltaTime;
                if (_interfaceAlpha < 0)
                {
                    _interfaceAlpha = 0;
                    _interfaceCurrentState = false;
                    foreach (var interfaceGameObject in _interfaceGameObjects)
                    {
                        interfaceGameObject.SetActive(false);
                    }
                }
                foreach (var interfaceImage in _interfaceImages)
                {
                    ChangeAlpha(interfaceImage, _interfaceAlpha);
                }
                foreach (var interfaceText in _interfaceTexts)
                {
                    ChangeAlpha(interfaceText, _interfaceAlpha);
                }
            }
        }
    }

    private void ChangeAlpha(Image image, float newAlpha)
    {
        Color oldColor = image.color;
        image.color = new Color(oldColor.r, oldColor.g, oldColor.b, newAlpha);
    }

    private void ChangeAlpha(TextMeshProUGUI text, float newAlpha)
    {
        Color oldColor = text.color;
        text.color = new Color(oldColor.r, oldColor.g, oldColor.b, newAlpha);
    }

    public void ChangeRespawnProgress(float newProgress)
    {
        TranslatedText respawningText = new()
        {
            RussianText = "Возрождение через: ",
            EnglishText = "Respawning through: "
        };
        _respawnText.text = respawningText.GetTranslatedString() + Mathf.RoundToInt(newProgress);
    }
}
