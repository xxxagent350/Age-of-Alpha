using UnityEngine;
using Unity.Netcode;

public abstract class NetworkAuthorityChecker : NetworkBehaviour
{
    //���� ���� �������� ������ ���� ��� ������ �������� ������: ���� ���������� ������-�� ������� ��� ������(�� ����), �� ������ ���������� � ������ �� ��� ���������(��������, ������������ �������), �� Netcode �������� ������� ���������� ������� ������, ��� � ����� ������ ����������� (����� ��������� ������� �� ���� ���� ��������� ������������ � ����� ������� � ������� ������������ ������� � �. �.)
    private Player _myPlayer;
    private bool _activated = false;

    public void ActivateNetworkAuthorityChecker(Player myPlayer)
    {
        _myPlayer = myPlayer;
        _activated = true;
    }

    public bool OnOwner()
    {
        if (_activated == false)
        {
            Debug.LogError($"{gameObject}: ����� ����������������� NetworkAuthorityChecker, ��� ��������� ������� ��������� �� �� ����� ActivateNetworkAuthorityChecker(Player myPlayer), ������� � ���� ��������� Player ������, �������� ����������� ���� ������");
        }

        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log("Server");
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
