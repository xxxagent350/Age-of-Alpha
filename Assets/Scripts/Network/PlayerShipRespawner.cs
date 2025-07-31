using Unity.Netcode;
using UnityEngine;

public class PlayerShipRespawner : NetworkBehaviour
{
    [Header("Настройка")]
    [SerializeField] private float _respawnTime = 5;
    [SerializeField] private Player _myPlayer;

    private float _currentRespawnTime;
    private bool _respawningPlayerShip;

    public void ActivateRespawnTimer()
    {
        EnableRespawningInterfaceRpc();
        _respawningPlayerShip = true;
        _currentRespawnTime = _respawnTime;
        ChangeRespawnProgressOnClientRpc(_currentRespawnTime);
    }

    private void FixedUpdate()
    {
        if (_respawningPlayerShip)
        {
            if (_currentRespawnTime > 0)
            {
                _currentRespawnTime -= Time.deltaTime;
                ChangeRespawnProgressOnClientRpc(_currentRespawnTime);
            }
            else
            {
                _currentRespawnTime = _respawnTime;
                _respawningPlayerShip = false;
                RespawnPlayerShip();
            }
        }
    }

    private void RespawnPlayerShip()
    {
        if (_myPlayer != null)
        {
            _myPlayer.SpawnPlayerShip();
        }
    }

    [Rpc(SendTo.Owner)]
    private void EnableRespawningInterfaceRpc()
    {
        GameInterfaceManager.Instance.EnableRespawnInterface();
    }

    [Rpc(SendTo.Owner)]
    private void ChangeRespawnProgressOnClientRpc(float newProgress)
    {
        GameInterfaceManager.Instance.RespawnInterfaceManager.ChangeRespawnProgress(newProgress);
    }
}
