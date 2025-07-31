using UnityEngine;
using Unity.Netcode;

public abstract class NetworkAuthorityChecker : NetworkBehaviour
{
    //���� ���� �������� ������ ���� ��� ������ �������� ������: ���� ���������� ������-�� ������� ��� ������(�� ����), �� ������ ���������� � ������ �� ��� ���������(��������, ������������ �������), �� Netcode �������� ������� ���������� ������� ������, ��� � ����� ������ ����������� (����� ��������� ������� �� ���� ���� ��������� ������������ � ����� ������� � ������� ������������ ������� � �. �.)
    private Player _myPlayer;
    public bool _networkAuthorityCheckerActivated { get; private set; } = false;
    private bool _deactivated = false;

    public void ActivateNetworkAuthorityChecker(Player myPlayer)
    {
        if (_networkAuthorityCheckerActivated == false)
        {
            _myPlayer = myPlayer;
            _networkAuthorityCheckerActivated = true;
        }
    }

    public void DeactivateNetworkAuthorityChecker()
    {
        //������ ����� ���������� false
        _deactivated = true;
    }

    public bool OnOwner()
    {
        if (_deactivated)
        {
            return false;
        }

        if (_networkAuthorityCheckerActivated == false)
        {
            Debug.LogError($"{gameObject}: ����� ����������������� NetworkAuthorityChecker, ��� ��������� ������� ��������� �� �� ����� ActivateNetworkAuthorityChecker(Player myPlayer), ������� � ���� ��������� Player ������, �������� ����������� ���� ������");
        }

        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                if (_myPlayer == null)
                {
                    return false;
                }
                else
                {
                    return IsOwner;
                }
            }
            else
            {
                return IsOwner;
            }
        }
        return false;
    }
}
