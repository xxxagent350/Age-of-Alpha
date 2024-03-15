using Unity.Multiplayer.Tools.NetStatsMonitor;
using Unity.Netcode;
using UnityEngine;

public class NetworkManagerController : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] GameObject[] initializeNetworkButtons;
    [SerializeField] GameObject stopNetworkButton;

    [Header("Отладка")]
    [SerializeField] RuntimeNetStatsMonitor networkStatsMonitor;

    bool connectButtonsActive = true;

    private void Awake()
    {
        networkStatsMonitor = GameObject.Find("NETWORK MANAGER").GetComponent<RuntimeNetStatsMonitor>();
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
    public void Stop()
    {
        NetworkManager.Singleton.Shutdown();
    }

    private void Update()
    {
        bool serverRunning = NetworkManager.Singleton.IsServer;
        bool clientRunning = NetworkManager.Singleton.IsClient;

        if (serverRunning || clientRunning)
        {
            SetActiveNetworkButtons(false);
        }
        else
        {
            SetActiveNetworkButtons(true);
        }

        if (serverRunning && !clientRunning)
        {
            networkStatsMonitor.enabled = true;
        }
        else
        {
            networkStatsMonitor.enabled = false;
        }
    }

    void SetActiveNetworkButtons(bool setConnectButtonsActive)
    {
        if (setConnectButtonsActive != connectButtonsActive)
        {
            for (int buttonNum = 0; buttonNum < initializeNetworkButtons.Length; buttonNum++)
            {
                initializeNetworkButtons[buttonNum].SetActive(setConnectButtonsActive);
            }
            stopNetworkButton.SetActive(!setConnectButtonsActive);
            connectButtonsActive = setConnectButtonsActive;
        }
    }
}
