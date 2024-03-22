using UnityEngine;
using Unity.Netcode;

public class ShipGameStats : NetworkBehaviour
{
    [Header("Отладка")]
    public float mass; //масса корпуса с модулями
    public float energyGeneration; //суммарная генерация энергии со всех модулей
    public NetworkVariable<float> energyMaxCapacity; //максимальное количество запасаемой энергии во всех модулях
    public NetworkVariable<float> energy; //текущее количество энергии в батареях
    public float accelerationPower; //общая ускорительная мощь двигателей
    public float angularAccelerationPower; //общая угловая ускорительная мощь двигателей
    public float speed; //скорость и ускорение
    public float angularSpeed; //угловая скорость и ускорение

    ShipStats myShipStats;
    EnergyBar energyBar;

    private void Start()
    {
        if (IsOwner)
        {
            energyBar = PlayerInterface.instance.energyBar;
            PlayerInterface.instance.playerInterfaceEnabled = true;
            FindFirstObjectByType<CameraMover>().playerTransform = transform;
        }
    }

    override public void OnDestroy()
    {
        if (IsOwner)
        {
            PlayerInterface.instance.playerInterfaceEnabled = false;
        }
    }

    [Rpc(SendTo.Everyone)]
    public void InitializeRpc()
    {
        enabled = true;
        if (NetworkManager.Singleton.IsServer)
        {
            myShipStats = GetComponent<ShipStats>();

            mass = myShipStats.totalMass;
            energyGeneration = myShipStats.totalEnergyGeneration;
            energyMaxCapacity.Value = myShipStats.totalEnergyCapacity;
            energy.Value = energyMaxCapacity.Value;

            accelerationPower = myShipStats.totalAccelerationPower;
            angularAccelerationPower = myShipStats.totalAngularAccelerationPower;
        }
    }

    private void FixedUpdate()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            GenerateEnergy();
        }
        if (IsOwner)
        {
            if (energyMaxCapacity.Value > 0.001f)
            {
                energyBar.fillingValue = energy.Value / energyMaxCapacity.Value;
            }
            else
            {
                energyBar.fillingValue = 0;
            }
        }
    }

    void GenerateEnergy()
    {
        if (energy.Value < energyMaxCapacity.Value)
        {
            energy.Value += energyGeneration * Time.deltaTime;
        }
        if (energy.Value > energyMaxCapacity.Value)
        {
            energy.Value = energyMaxCapacity.Value;
        }
        if (energy.Value < 0)
        {
            energy.Value = 0;
        }
    }
}
