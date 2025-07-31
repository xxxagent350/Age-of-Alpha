using UnityEngine;
using Unity.Netcode;

public class KamikadzeMode : NetworkAuthorityChecker
{
    [Header("Настройка")]
    [Tooltip("Корабль будет плавно становиться такого цвета когда делает камикадзе")]
    [SerializeField] private Color _kamikadzingShipColor = Color.red;
    [SerializeField] private float _timeToExplode = 2;

    private ShipGameStats _shipGameStats;
    private SpriteRenderer _shipsSpriteRenderer;
    private NetworkVariable<bool> _kamikadzeModeEnabled = new();
    private float _timerToExplode;

    private void Start()
    {
        _shipGameStats = GetComponent<ShipGameStats>();
        _shipsSpriteRenderer = GetComponent<ItemData>().Image.GetComponent<SpriteRenderer>();
        ActivateNetworkAuthorityChecker(GetComponent<ShipGameStats>().MyPlayer);

        if (OnOwner())
        {
            ShipInterfaceManager.Instance.KamikadzeButton.KamikadzeModeEnabledEvent += EnableKamikadzeMode_Client;
        }
    }

    public override void OnDestroy()
    {
        if (OnOwner())
        {
            ShipInterfaceManager.Instance.KamikadzeButton.KamikadzeModeEnabledEvent -= EnableKamikadzeMode_Client;
        }
    }

    private void EnableKamikadzeMode_Client()
    {
        StartingKamikadzeModeRpc();
    }

    [Rpc(SendTo.Server)]
    private void StartingKamikadzeModeRpc()
    {
        _kamikadzeModeEnabled.Value = true;
    }

    private void Update()
    {
        if (_shipGameStats.Destroyed.Value == false)
        {
            if (_kamikadzeModeEnabled.Value)
            {
                if (_timerToExplode < _timeToExplode)
                {
                    _timerToExplode += Time.deltaTime;
                    _shipsSpriteRenderer.color = (Color.white * (1 - (_timerToExplode / _timeToExplode)))
                        + (_kamikadzingShipColor * (_timerToExplode / _timeToExplode));
                }
                else
                {
                    if (NetworkManager.Singleton.IsServer)
                    {
                        _kamikadzeModeEnabled.Value = false;
                        _shipGameStats.ControlBlock.Explode();
                    }
                }
            }
            else if (_shipGameStats.Destroyed.Value)
            {
                _shipsSpriteRenderer.color = Color.white;
                _timerToExplode = 0;
            }
        }
    }
}
