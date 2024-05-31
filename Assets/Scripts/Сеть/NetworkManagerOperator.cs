using Unity.Multiplayer.Tools.NetStatsMonitor;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;

public class NetworkManagerOperator : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] GameObject[] initializeNetworkButtons;
    [SerializeField] GameObject stopNetworkButton;
    [SerializeField] InputField ipEnterField;

    [Header("Отладка")]
    [SerializeField] RuntimeNetStatsMonitor networkStatsMonitor;

    bool connectButtonsActive = true;

    private void Awake()
    {
        networkStatsMonitor = GameObject.Find("NETWORK MANAGER").GetComponent<RuntimeNetStatsMonitor>();
        if (!PlayerPrefs.HasKey("LastTimeConnectingIP"))
            SetIP("10.144.14.197");
        ipEnterField.text = PlayerPrefs.GetString("LastTimeConnectingIP");
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

    public void SetIP(string IP)
    {
        PlayerPrefs.SetString("LastTimeConnectingIP", IP);
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = IP;
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
