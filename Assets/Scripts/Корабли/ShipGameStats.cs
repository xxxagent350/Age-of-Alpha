using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

public class ShipGameStats : NetworkBehaviour
{
    [Header("Настройка")]
    [SerializeField] float enginesLightsAlphaChangingSpeed = 2;
    [SerializeField] List<SpriteRenderer> enginesLights;
    [SerializeField] List<TrailRenderer> trails;

    [Header("Отладка")]
    public float mass; //масса корпуса с модулями
    public NetworkVariable<float> energyGeneration; //суммарная генерация энергии со всех модулей
    public NetworkVariable<float> energyMaxCapacity; //максимальное количество запасаемой энергии во всех модулях
    public NetworkVariable<float> energy; //текущее количество энергии в батареях
    public NetworkVariable<float> enginesConsumption; //макс. потребление двигателями при полёте
    public float accelerationPower; //общая ускорительная мощь двигателей
    public float angularAccelerationPower; //общая угловая ускорительная мощь двигателей

    ShipStats myShipStats;
    ItemData myItemData;
    EnergyBar energyBar;
    Rigidbody2D myRigidbody2D;
    float enginesLightsAlpha;
    [SerializeField] bool noEnergy;

    //данные с джойстика игрока
    [HideInInspector] public NetworkVariable<bool> movementJoystickPressed;
    [HideInInspector] public NetworkVariable<float> movementJoystickDirInDegrees;
    [HideInInspector] public NetworkVariable<float> movementJoystickMagnitude;

    //модификаторы полёта
    const float ignoredDirDifferenceDegrees = 60;
    const float minDrag = 0;
    const float maxSpeedMod = 150;
    const float linearDragMod = 0.01f;
    const float accelerationPowerMod = 500;
    
    bool noControl; //true когда нету блока управления

    private void Start()
    {
        if (!DataOperator.gameScene)
        {
            return;
        }
        if (IsOwner)
        {
            energyBar = PlayerInterface.instance.energyBar;
            CameraMover.instance.SetPlayerShip(transform);
        }
        if (NetworkManager.Singleton.IsServer)
        {
            myRigidbody2D = GetComponent<Rigidbody2D>();
            myRigidbody2D.mass = mass;
        }
        foreach (SpriteRenderer engineLight in enginesLights)
        {
            engineLight.gameObject.SetActive(true);
        }
    }

    public override void OnDestroy()
    {
        if (DataOperator.gameScene && IsOwner)
        {
            PlayerInterface.instance.SetActivePlayerInterface(false);
        }
    }

    public void ServerInitialize()
    {
        myShipStats = GetComponent<ShipStats>();

        mass = myShipStats.totalMass;
        if (myRigidbody2D == null)
        {
            myRigidbody2D = GetComponent<Rigidbody2D>();
        }

        myRigidbody2D.mass = mass;

        energyGeneration.Value = myShipStats.totalEnergyGeneration;
        energyMaxCapacity.Value = myShipStats.totalEnergyCapacity;
        energy.Value = energyMaxCapacity.Value;

        enginesConsumption.Value = myShipStats.totalEnginesConsumption;
        accelerationPower = myShipStats.totalAccelerationPower;
        angularAccelerationPower = myShipStats.totalAngularAccelerationPower;

        Invoke(nameof(WaitingToBeSpawned), 0.1f);
    }

    void WaitingToBeSpawned()
    {
        if (IsSpawned)
        {
            if (myShipStats.ControlBlockExists())
            {
                OwnerInitializeRpc(true);
            }
            else
            {
                noControl = true;
                OwnerInitializeRpc(false);
            }
        }
        else
        {
            Invoke(nameof(WaitingToBeSpawned), 0.1f);
        }
    }

    private void FixedUpdate()
    {
        if (DataOperator.gameScene)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                if (!noControl)
                {
                    RotateShip();
                    Accelerate();
                }
                GenerateEnergy();
            }
            if (IsOwner)
            {
                OwnerUI();
            }
            FlightEffects();
        }
    }

    void OwnerUI()
    {
        if (noEnergy)
        {
            energyBar.fillingValue = 0;
        }
        else
        {
            if (energy.Value + (energyGeneration.Value * Time.deltaTime) >= energyMaxCapacity.Value)
            {
                energyBar.fillingValue = 1;
            }
            else
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
    }

    void FlightEffects()
    {
        float enginesLightsAlphaFrameChange = enginesLightsAlphaChangingSpeed * Time.deltaTime;

        if (movementJoystickPressed.Value && Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, movementJoystickDirInDegrees.Value)) < ignoredDirDifferenceDegrees)
        {
            float enginesLightsMod;

            if (CheckEnergy(((enginesConsumption.Value * movementJoystickMagnitude.Value) + energyGeneration.Value) * Time.deltaTime))
            {
                enginesLightsMod = movementJoystickMagnitude.Value;
            }
            else
            {
                enginesLightsMod = movementJoystickMagnitude.Value * (energyGeneration.Value / (enginesConsumption.Value * movementJoystickMagnitude.Value));
            }

            if (CheckEnergy(enginesConsumption.Value * movementJoystickMagnitude.Value * Time.deltaTime))
            {
                foreach (TrailRenderer trail in trails)
                {
                    trail.emitting = true;
                }
            }
            else
            {
                foreach (TrailRenderer trail in trails)
                {
                    trail.emitting = false;
                }
            }

            if (enginesLightsAlpha < enginesLightsMod)
            {
                enginesLightsAlpha += enginesLightsAlphaFrameChange;
            }
            if (enginesLightsAlpha > enginesLightsMod)
            {
                enginesLightsAlpha -= enginesLightsAlphaFrameChange;
            }
            if (Mathf.Abs(enginesLightsAlpha - enginesLightsMod) < enginesLightsAlphaFrameChange)
            {
                enginesLightsAlpha = enginesLightsMod;
            }
        }
        else
        {
            foreach (TrailRenderer trail in trails)
            {
                trail.emitting = false;
            }
            if (enginesLightsAlpha > 0)
            {
                enginesLightsAlpha -= enginesLightsAlphaFrameChange;
            }
        }

        foreach (SpriteRenderer engineLight in enginesLights)
        {
            Color oldColor = engineLight.color;
            engineLight.color = new Color(oldColor.r, oldColor.g, oldColor.b, enginesLightsAlpha);
        }
    }


    void GenerateEnergy()
    {
        if (energy.Value <= energyGeneration.Value * Time.deltaTime)
        {
            noEnergy = true;
        }
        else
        {
            noEnergy = false;
        }
        if (energy.Value < energyMaxCapacity.Value)
        {
            energy.Value += energyGeneration.Value * Time.deltaTime;
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

    public bool TakeEnergy(float energyAmount)
    {
        if (energy.Value < energyAmount)
        {
            return false;
        }
        else
        {
            energy.Value -= energyAmount;
            return true;
        }
    }

    public bool CheckEnergy(float energyAmount)
    {
        if (energy.Value < energyAmount)
        {
            return false;
        }
        else
        {
            return true;
        }
    }


    void RotateShip()
    {
        const float rotationForceMod = 50000;

        float F = angularAccelerationPower * rotationForceMod; //крутящий момент (аналог силы)
        float m = myRigidbody2D.inertia; //момент инерции (аналог массы)

        float v2 = Mathf.Abs(Mathf.Pow(myRigidbody2D.angularVelocity, 2)); //угловая скорость в квадрате
        if (myRigidbody2D.angularVelocity < 0)
        {
            v2 *= -1;
        }

        float a = F / m; //угловое ускорение
        float S = Mathf.DeltaAngle(transform.eulerAngles.z, movementJoystickDirInDegrees.Value);

        //Debug.Log($"S: {S}; v2: {v2}; a: {a}");

        float ignoredDir = a * Mathf.Pow(Time.deltaTime * 2, 2);
        if (Mathf.Abs(S) < ignoredDir && Mathf.Abs(myRigidbody2D.angularVelocity) < a / Time.deltaTime * 2)
        {
            transform.eulerAngles = new Vector3(0, 0, movementJoystickDirInDegrees.Value);
            myRigidbody2D.angularVelocity = 0;
        }
        else
        {
            if (TakeEnergy(enginesConsumption.Value * Time.deltaTime))
            {
                if (S > 0)
                {
                    if (v2 < 2 * a * S * 0.9f) //ещё не разогнались достаточно, продолжаем ускоряться
                    {
                        myRigidbody2D.AddTorque(F * Time.deltaTime);
                    }
                    else //тормозим дабы не возникло колебаний
                    {
                        myRigidbody2D.AddTorque(-F * Time.deltaTime * 0.9f);
                    }
                }
                if (S < 0)
                {
                    if (v2 > 2 * a * S * 0.9f) //ещё не разогнались достаточно, продолжаем ускоряться
                    {
                        myRigidbody2D.AddTorque(-F * Time.deltaTime);
                    }
                    else //тормозим дабы не возникло колебаний
                    {
                        myRigidbody2D.AddTorque(F * Time.deltaTime * 0.9f);
                    }
                }
            } 
        }
    }

    void Accelerate()
    {
        Vector3 velocity = myRigidbody2D.velocity;
        float maxSpeed = accelerationPower * maxSpeedMod / mass;

        if (velocity.magnitude > maxSpeed)
        {
            myRigidbody2D.drag = minDrag + ((velocity.magnitude - maxSpeed) * linearDragMod);
        }
        else
        {
            myRigidbody2D.drag = minDrag;
        }


        if (movementJoystickPressed.Value)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, movementJoystickDirInDegrees.Value)) < ignoredDirDifferenceDegrees)
            {
                if (TakeEnergy(enginesConsumption.Value * movementJoystickMagnitude.Value * Time.deltaTime))
                {
                    myRigidbody2D.AddForce(DataOperator.RotateVector2(new Vector2(0, accelerationPower * accelerationPowerMod * movementJoystickMagnitude.Value), movementJoystickDirInDegrees.Value));
                }
            }
        }
        else
        {
            myRigidbody2D.drag = minDrag;
        }
    }

    [Rpc(SendTo.Owner)]
    void OwnerInitializeRpc(bool controlBlockExists)
    {
        myItemData = GetComponent<ItemData>();
        Vector3 extremePoints = myItemData.GetMaxSlotsPosition() - myItemData.GetMinSlotsPosition();
        float shipSize = extremePoints.magnitude;

        GameCameraScaler.instance.minZoom = shipSize * 2;
        GameCameraScaler.instance.zoom = shipSize * 5;

        if (controlBlockExists)
        {
            PlayerInterface.instance.SetActivePlayerInterface(true);
        }
        else
        {
            TranslatedText warningMessage = new TranslatedText
            {
                RussianText = "Нет связи с кораблём: не установлен блок управления",
                EnglishText = "No communication with the ship: no control block installed"
            };
            PlayerInterface.instance.ShowWarningText(warningMessage);
        }
    }


    public delegate void AttackButtonStateChangedMessage(uint index, bool pressed);
    public event AttackButtonStateChangedMessage attackButtonStateChangedMessage;

    public void SendFireStateChange(uint index, bool fire)
    {
        attackButtonStateChangedMessage?.Invoke(index, fire);
    }
}
