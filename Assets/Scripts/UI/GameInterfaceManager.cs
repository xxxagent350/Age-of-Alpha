using UnityEngine;

public class GameInterfaceManager : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] private PauseInterfaceManager _pauseInterfaceManager;
    [SerializeField] private ShipInterfaceManager _shipInterfaceManager;

    public static GameInterfaceManager Instance;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void EnablePauseInterface()
    {
        DisableAllInterfaces();
        _pauseInterfaceManager.EnableInterface();
    }

    public void EnableShipInterface()
    {
        DisableAllInterfaces();
        _shipInterfaceManager.SetActiveInterface(true);
    }

        
    public void DisableAllInterfaces()
    {
        _pauseInterfaceManager.DisableInterface();
        _shipInterfaceManager.SetActiveInterface(false);
    }
}
