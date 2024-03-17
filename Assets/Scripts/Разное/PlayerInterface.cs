using UnityEngine;
using UnityEngine.UI;

public class PlayerInterface : MonoBehaviour
{
    [Header("Настройка")]
    public EnergyBar energyBar;
    [SerializeField] GameObject[] playerInterfaceGameObjects;
    [SerializeField] Image[] playerInterfaceImages;
    [SerializeField] float playerInterfaceAlphaChangingSpeed = 5f;

    [Header("Отладка")]
    public bool playerInterfaceEnabled;

    public static PlayerInterface instance;
    bool playerInterfaceGameObjectsEnabled;
    float playerInterfaceAlpha;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
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
                foreach (Image intefaceElement in playerInterfaceImages)
                {
                    Color oldColor = intefaceElement.color;
                    intefaceElement.color = new Color(oldColor.r, oldColor.g, oldColor.b, playerInterfaceAlpha);
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
                foreach (Image intefaceElement in playerInterfaceImages)
                {
                    Color oldColor = intefaceElement.color;
                    intefaceElement.color = new Color(oldColor.r, oldColor.g, oldColor.b, playerInterfaceAlpha);
                }
            }
        }
    }
}
