using UnityEngine;

public class GameInterfaceManager : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] private GameObject NetworkInterface;
    [SerializeField] private GameObject PauseInterface;
    [SerializeField] private GameObject ShipInterface;

    public void EnableMetworkInterface()
    {
        DisableAllInterfaces();
        NetworkInterface.SetActive(true);
    }

    public void EnablePauseInterface()
    {
        DisableAllInterfaces();
        PauseInterface.SetActive(true);
    }

    public void EnableShipInterface()
    {
        DisableAllInterfaces();
        ShipInterface.SetActive(true);
    }


    private void DisableAllInterfaces()
    {
        NetworkInterface.SetActive(false);
        PauseInterface.SetActive(false);
        ShipInterface.SetActive(false);
    }
}
