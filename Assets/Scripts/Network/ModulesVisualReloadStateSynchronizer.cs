using System.Collections.Generic;
using Unity.Netcode;

public class ModulesVisualReloadStateSynchronizer : NetworkBehaviour
{
    private ShipGameStats _myShipGameStats;
    private Dictionary<uint, List<float>> _reloadProgresses = new();
    private Dictionary<uint, AttackButton> _clientsAttackButtons = new();

    private void Start()
    {
        _myShipGameStats = GetComponent<ShipGameStats>();
        _myShipGameStats.ActivateNetworkAuthorityChecker(_myShipGameStats.MyPlayer);
        if (_myShipGameStats.OnOwner())
        {
            foreach (var attackButton in ShipInterfaceManager.Instance.AttackButtons)
            {
                _clientsAttackButtons.Add(attackButton.ButtonIndex, attackButton);
            }
        }
    }

    private void FixedUpdate()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            foreach (var reloadProgresses in _reloadProgresses)
            {
                if (reloadProgresses.Value.Count > 0)
                {
                    float bestReloadProgress = 0;
                    //for (int rechargeProgressNum = 0; rechargeProgressNum < reloadProgresses.Value.Count; rechargeProgressNum++)
                    while (reloadProgresses.Value.Count > 0)
                    {
                        if (reloadProgresses.Value[0] > bestReloadProgress)
                        {
                            bestReloadProgress = reloadProgresses.Value[0];
                        }
                        reloadProgresses.Value.RemoveAt(0);
                    }
                    SendNewButtonReloadProgressRpc(reloadProgresses.Key, bestReloadProgress);
                }
            }
        }
    }

    public void AddRechargeProgressData(uint weaponIndex, float newRechargeProgress)
    {
        List<float> reloadProgressesForWeaponIndex;
        if (_reloadProgresses.TryGetValue(weaponIndex, out reloadProgressesForWeaponIndex) == false)
        {
            reloadProgressesForWeaponIndex = new();
            _reloadProgresses.Add(weaponIndex, reloadProgressesForWeaponIndex);
        }
        reloadProgressesForWeaponIndex.Add(newRechargeProgress);
    }

    [Rpc(SendTo.Owner)]
    private void SendNewButtonReloadProgressRpc(uint buttonIndex, float reloadProgress)
    {
        if (_myShipGameStats != null && _myShipGameStats.OnOwner())
        {
            _clientsAttackButtons[buttonIndex].SetVisualReloadProgress(reloadProgress);
        }
    }
}
