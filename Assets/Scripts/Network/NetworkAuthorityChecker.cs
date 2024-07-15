using UnityEngine;
using Unity.Netcode;

public abstract class NetworkAuthorityChecker : NetworkBehaviour
{
    //весь этот компонет сделан лишь для одного частного случая: если владельцем какого-то объекта был клиент(не хост), но клиент отключился и объект не был уничтожен(например, уничтоженный корабль), то Netcode начинает считать владельцем объекта сервер, что в нашем случае неприемлемо (могут возникать приколы по типу хост управляет одновременно и своим кораблём и кораблём отключённого клиента и т. д.)
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
            Debug.LogError($"{gameObject}: вызов неактивированного NetworkAuthorityChecker, для активации сначала выполните на нём метод ActivateNetworkAuthorityChecker(Player myPlayer), передав в него компонент Player игрока, которому принадлежит этот объект");
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
