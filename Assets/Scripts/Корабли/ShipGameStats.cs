using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ShipGameStats : NetworkBehaviour
{
    [Header("Настройка")]
    [SerializeField] float enginesLightsAlphaChangingSpeed = 2;
    [SerializeField] List<SpriteRenderer> enginesLights;
    [SerializeField] List<TrailRenderer> trails;

    [SerializeField] List<Effect> destroyEffects;
    [SerializeField] Sprite destroyedImage;
    [SerializeField] float timeToDisappearAfterDestroy;
    [Tooltip("Импульс при уничтожении корабля")]
    [SerializeField] float forceOnDestroy;
    [Tooltip("Вращательный импульс при уничтожении корабля")]
    [SerializeField] float rotationForceOnDestroy;

    [Header("Отладка")]
    public string TeamID;
    public float Mass; //масса корпуса с модулями
    public NetworkVariable<float> EnergyGeneration = new NetworkVariable<float>(); //суммарная генерация энергии со всех модулей
    public NetworkVariable<float> EnergyMaxCapacity = new NetworkVariable<float>(); //максимальное количество запасаемой энергии во всех модулях
    public NetworkVariable<float> Energy = new NetworkVariable<float>(); //текущее количество энергии в батареях
    public NetworkVariable<float> EnginesConsumption = new NetworkVariable<float>(); //макс. потребление двигателями при полёте
    public float AccelerationPower; //общая ускорительная мощь двигателей
    public float AngularAccelerationPower; //общая вращательная мощь двигателей

    ShipStats myShipStats;
    ItemData myItemData;
    EnergyBar energyBar;
    Rigidbody2D myRigidbody2D;
    [SerializeField] NetworkVariable<bool> noEnergy = new NetworkVariable<bool>();

    //данные с джойстика игрока
    [HideInInspector] public NetworkVariable<bool> MovementJoystickPressed = new NetworkVariable<bool>();
    [HideInInspector] public NetworkVariable<float> MovementJoystickDirInDegrees = new NetworkVariable<float>();
    [HideInInspector] public NetworkVariable<float> MovementJoystickMagnitude = new NetworkVariable<float>();

    //модификаторы полёта
    private const float IgnoredDirDifferenceDegrees = 60;
    private const float MinDrag = 0;
    private const float MaxSpeedMod = 150;
    private const float LinearDragMod = 0.3f;
    private const float AccelerationPowerMod = 1600;
    private const float RotationForceMod = 75000;

    bool noControl; //true когда нету блока управления
    NetworkVariable<bool> alreadyDestroyed = new NetworkVariable<bool>();
    SpriteRenderer shipsSpriteRenderer;

    [HideInInspector] public Player myPlayer;

    NetworkVariable<float> enginesLightsAlpha = new NetworkVariable<float>();
    NetworkVariable<bool> trailsEmitting = new NetworkVariable<bool>();

    private void Start()
    {
        myItemData = GetComponent<ItemData>();

        if (!DataOperator.gameScene)
        {
            return;
        }
        if (IsOwner)
        {
            energyBar = PlayerInterface.instance.energyBar;
            CameraMover.instance.SetPlayerShip(transform);
        }
        foreach (SpriteRenderer engineLight in enginesLights)
        {
            engineLight.gameObject.SetActive(true);
        }
        shipsSpriteRenderer = myItemData.image.GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        StartCoroutine(WaitingForBeingDestroyed());
    }

    IEnumerator WaitingForBeingDestroyed()
    {
        while (true)
        {
            if (alreadyDestroyed.Value)
            {
                DisableFlightEffects();
                shipsSpriteRenderer.sprite = destroyedImage;
                enabled = false;
                break;
            }
            yield return new WaitForSeconds(Time.fixedDeltaTime);
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

        Mass = myShipStats.totalMass;

        myRigidbody2D = GetComponent<Rigidbody2D>();
        myRigidbody2D.mass = Mass;

        EnergyGeneration.Value = myShipStats.totalEnergyGeneration;
        EnergyMaxCapacity.Value = myShipStats.totalEnergyCapacity;
        Energy.Value = EnergyMaxCapacity.Value;

        EnginesConsumption.Value = myShipStats.totalEnginesConsumption;
        AccelerationPower = myShipStats.totalAccelerationPower;
        AngularAccelerationPower = myShipStats.totalAngularAccelerationPower;

        Invoke(nameof(WaitingToBeSpawned), 0.1f);
    }

    void WaitingToBeSpawned()
    {
        if (IsSpawned)
        {
            OwnerInitializeRpc();
            if (myShipStats.ControlBlockExists())
            {
                ChangeOwnersInterfaceStateRpc(true);
            }
            else
            {
                noControl = true;
                ChangeOwnersInterfaceStateRpc(false);
                TranslatedText warningMessage = new TranslatedText
                {
                    RussianText = "Нет связи с кораблём: не установлен блок управления",
                    EnglishText = "No communication with the ship: no control block installed"
                };
                SendMessageToOwnerRpc(new TranslatedNetworkText(warningMessage));
                Invoke(nameof(ExplodeTheShipOnServer), 5f);
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
                if (myPlayer == null)
                {
                    //игрок, управляющий кораблём, отключился
                    ExplodeTheShipOnServer();
                    return;
                }
                if (!noControl)
                {
                    RotateShip();
                    Accelerate();
                    if (AccelerationPower > 0.01f)
                    {
                        ApplyFriction();
                    }
                }
                else
                {
                    myRigidbody2D.drag = MinDrag;
                }
                GenerateEnergy();
                FlightEffectsServer();
            }
            if (IsOwner)
            {
                OwnerUI();
            }
            FlightEffectsClient();
        }
    }

    void OwnerUI()
    {
        if (noEnergy.Value)
        {
            energyBar.fillingValue = 0;
        }
        else
        {
            if (Energy.Value + (EnergyGeneration.Value * Time.fixedDeltaTime) >= EnergyMaxCapacity.Value)
            {
                energyBar.fillingValue = 1;
            }
            else
            {
                if (EnergyMaxCapacity.Value > 0.001f)
                {
                    energyBar.fillingValue = Energy.Value / EnergyMaxCapacity.Value;
                }
                else
                {
                    energyBar.fillingValue = 0;
                }
            }
        }
    }

    void FlightEffectsServer()
    {
        float enginesLightsAlphaFrameChange = enginesLightsAlphaChangingSpeed * Time.fixedDeltaTime;

        if (AccelerationPower > 0.01f && !noControl && MovementJoystickPressed.Value && Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, MovementJoystickDirInDegrees.Value)) < IgnoredDirDifferenceDegrees)
        {
            float enginesLightsMod;

            if (CheckEnergy(((EnginesConsumption.Value * MovementJoystickMagnitude.Value) + EnergyGeneration.Value) * Time.fixedDeltaTime))
            {
                enginesLightsMod = MovementJoystickMagnitude.Value;
            }
            else
            {
                enginesLightsMod = MovementJoystickMagnitude.Value * (EnergyGeneration.Value / (EnginesConsumption.Value * MovementJoystickMagnitude.Value));
            }

            if (CheckEnergy(EnginesConsumption.Value * MovementJoystickMagnitude.Value * Time.fixedDeltaTime))
            {
                trailsEmitting.Value = true;
            }
            else
            {
                trailsEmitting.Value = false;
            }

            if (enginesLightsAlpha.Value < enginesLightsMod)
            {
                enginesLightsAlpha.Value += enginesLightsAlphaFrameChange;
            }
            if (enginesLightsAlpha.Value > enginesLightsMod)
            {
                enginesLightsAlpha.Value -= enginesLightsAlphaFrameChange;
            }
            if (Mathf.Abs(enginesLightsAlpha.Value - enginesLightsMod) < enginesLightsAlphaFrameChange)
            {
                enginesLightsAlpha.Value = enginesLightsMod;
            }
        }
        else
        {
            trailsEmitting.Value = false;
            if (enginesLightsAlpha.Value > 0)
            {
                enginesLightsAlpha.Value -= enginesLightsAlphaFrameChange;
            }
        }
    }

    void FlightEffectsClient()
    {
        foreach (SpriteRenderer engineLight in enginesLights)
        {
            Color oldColor = engineLight.color;
            engineLight.color = new Color(oldColor.r, oldColor.g, oldColor.b, enginesLightsAlpha.Value);
        }

        if (trailsEmitting.Value)
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
    }



    void GenerateEnergy()
    {
        if (Energy.Value <= EnergyGeneration.Value * Time.fixedDeltaTime || EnergyMaxCapacity.Value < 0.01f)
        {
            noEnergy.Value = true;
        }
        else
        {
            noEnergy.Value = false;
        }
        if (Energy.Value < EnergyMaxCapacity.Value)
        {
            Energy.Value += EnergyGeneration.Value * Time.fixedDeltaTime;
        }
        if (Energy.Value > EnergyMaxCapacity.Value)
        {
            Energy.Value = EnergyMaxCapacity.Value;
        }
        if (Energy.Value < 0)
        {
            Energy.Value = 0;
        }
    }

    public bool TrySpendEnergy(float amount)
    {
        if (amount <= 0)
            return false;

        if (Energy.Value < amount)
            return false;

        Energy.Value -= amount;

        return true;
    }

    public bool CheckEnergy(float energyAmount)
    {
        if (Energy.Value < energyAmount)
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
        float F = AngularAccelerationPower * RotationForceMod; //крутящий момент (аналог силы)
        float m = myRigidbody2D.inertia; //момент инерции (аналог массы)

        float v2 = Mathf.Abs(Mathf.Pow(myRigidbody2D.angularVelocity, 2)); //угловая скорость в квадрате
        if (myRigidbody2D.angularVelocity < 0)
        {
            v2 *= -1;
        }

        float a = F / m; //угловое ускорение
        float S = Mathf.DeltaAngle(transform.eulerAngles.z, MovementJoystickDirInDegrees.Value);

        //Debug.Log($"S: {S}; v2: {v2}; a: {a}; 2as: {2 * a * S}");

        float ignoredDir = a * Mathf.Pow(Time.fixedDeltaTime * 2, 2);

        if (Mathf.Abs(S) < ignoredDir && Mathf.Abs(myRigidbody2D.angularVelocity) < a / Time.fixedDeltaTime / 2)
        {
            transform.eulerAngles = new Vector3(0, 0, MovementJoystickDirInDegrees.Value);
            myRigidbody2D.angularVelocity = 0;
        }
        else
        {
            if (TrySpendEnergy(EnginesConsumption.Value * 0.3f * Time.fixedDeltaTime))
            {
                if (v2 < 2 * a * S) //ещё не разогнались достаточно, продолжаем ускоряться
                {
                    //Debug.Log("S < 0, ускоряемся");
                    myRigidbody2D.AddTorque(F * Time.fixedDeltaTime);
                }
                else //тормозим дабы не возникло колебаний
                {
                    //Debug.Log("S < 0, тормозим");
                    myRigidbody2D.AddTorque(-F * Time.fixedDeltaTime);
                }
            }
        }
    }

    void Accelerate()
    {
        if (MovementJoystickPressed.Value)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, MovementJoystickDirInDegrees.Value)) < IgnoredDirDifferenceDegrees)
            {
                if (TrySpendEnergy(EnginesConsumption.Value * MovementJoystickMagnitude.Value * Time.fixedDeltaTime))
                {
                    myRigidbody2D.AddForce(DataOperator.RotateVector2(new Vector2(0, AccelerationPower * AccelerationPowerMod * MovementJoystickMagnitude.Value), MovementJoystickDirInDegrees.Value));
                }
            }
        }
        else
        {
            myRigidbody2D.drag = MinDrag;
        }
    }

    void ApplyFriction()
    {
        Vector3 velocity = myRigidbody2D.velocity;
        float maxSpeed = Mathf.Pow(AccelerationPower / Mass, 1f / 2) * MaxSpeedMod;

        if (velocity.magnitude > maxSpeed)
        {
            myRigidbody2D.drag = MinDrag + ((velocity.magnitude - maxSpeed) * LinearDragMod);
        }
        else
        {
            myRigidbody2D.drag = MinDrag;
        }
    }

    public void ReduceShipСharacteristics(Durability destroyedModule)
    {
        ItemData itemData = destroyedModule.GetComponent<ItemData>();
        Battery battery = destroyedModule.GetComponent<Battery>();
        EnergyGenerator energyGenerator = destroyedModule.GetComponent<EnergyGenerator>();
        Engine engine = destroyedModule.GetComponent<Engine>();

        if (itemData != null)
        {
            Mass -= itemData.Mass;
            myRigidbody2D.mass = Mass;
            if (!alreadyDestroyed.Value && itemData.type == modulesTypes.ControlModules)
            {   //блок управления уничтожен
                ExplodeTheShipOnServer();
                TranslatedText warningMessage = new TranslatedText
                {
                    RussianText = "Связь с кораблём потеряна: блок управления уничтожен",
                    EnglishText = "Contact with the ship has been lost: the control unit has been destroyed"
                };
                SendMessageToOwnerRpc(new TranslatedNetworkText(warningMessage));
            }
        }
        if (battery != null)
        {
            Energy.Value -= (Energy.Value / EnergyMaxCapacity.Value) * battery.maxCapacity;
            EnergyMaxCapacity.Value -= battery.maxCapacity;
        }
        if (energyGenerator != null)
        {
            EnergyGeneration.Value -= energyGenerator.power;
        }
        if (engine != null)
        {
            EnginesConsumption.Value -= engine.powerConsumption;
            AccelerationPower -= engine.accelerationPower;
            AngularAccelerationPower -= engine.angularPower;
        }
    }

    void DisconnectFromTheShip()
    {
        ChangeOwnersInterfaceStateRpc(false);
    }

    void ExplodeTheShipOnServer()
    {
        if (!alreadyDestroyed.Value)
        {
            //отключаем все ещё оставшиеся системы корабля
            enabled = false;
            myRigidbody2D.drag = MinDrag;
            if (myPlayer != null)
            {
                DisconnectFromTheShip();
            }
            noControl = true;
            Weapon[] weapons = GetComponentsInChildren<Weapon>();
            foreach (Weapon weapon in weapons)
            {
                weapon.Disconnect();
            }

            alreadyDestroyed.Value = true;
            float maxForce = forceOnDestroy * Mass;
            Vector2 force = new Vector2(Random.Range(-maxForce, maxForce), Random.Range(-maxForce, maxForce));
            myRigidbody2D.AddForce(force);

            float maxRotationForce = rotationForceOnDestroy * Mass;
            float rotationForce = Random.Range(-maxRotationForce, maxRotationForce);
            myRigidbody2D.AddTorque(rotationForce);

            DisableFlightEffectsRpc();
            ShowDestroyEffectsRpc(myRigidbody2D.velocity);
        }
    }

    void DisableFlightEffects()
    {
        foreach (SpriteRenderer engineLight in enginesLights)
        {
            Color oldColor = engineLight.color;
            engineLight.color = new Color(oldColor.r, oldColor.g, oldColor.b, 0);
        }
        foreach (TrailRenderer trail in trails)
        {
            trail.emitting = false;
        }
    }

    [Rpc(SendTo.Everyone)]
    void DisableFlightEffectsRpc()
    {
        DisableFlightEffects();
        enabled = false;
    }

    [Rpc(SendTo.Everyone)]
    void ShowDestroyEffectsRpc(Vector3 effectsSpeed)
    {
        foreach (Effect effect in destroyEffects)
        {
            List<GameObject> spawnedGOs = effect.SpawnEffectsFromPool(transform.position, Quaternion.identity);
            foreach (GameObject spawnedGO in spawnedGOs)
            {
                spawnedGO.GetComponent<PooledEffect>().speed = effectsSpeed;
            }
        }
    }

    [Rpc(SendTo.Owner)]
    void OwnerInitializeRpc()
    {
        if (myItemData == null)
        {
            myItemData = GetComponent<ItemData>();
        }
        Vector3 extremePoints = myItemData.GetMaxSlotsPosition() - myItemData.GetMinSlotsPosition();
        float shipSize = extremePoints.magnitude;
        GameCameraScaler.instance.SetCameraLimits(shipSize);
    }

    [Rpc(SendTo.Owner)]
    void SendMessageToOwnerRpc(TranslatedNetworkText networkMessage)
    {
        TranslatedText message = networkMessage.GetTranslatedText();
        PlayerInterface.instance.ShowWarningText(message);
    }

    [Rpc(SendTo.Owner)]
    void ChangeOwnersInterfaceStateRpc(bool state)
    {
        PlayerInterface.instance.SetActivePlayerInterface(state);
    }


    public delegate void AttackButtonStateChangedMessage(uint index, bool pressed);
    public event AttackButtonStateChangedMessage attackButtonStateChangedMessage;

    public void SendFireStateChange(uint index, bool fire)
    {
        attackButtonStateChangedMessage?.Invoke(index, fire);
    }
}
