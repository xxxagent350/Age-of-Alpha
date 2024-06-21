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
    public string teamID;
    public float mass; //масса корпуса с модулями
    public NetworkVariable<float> energyGeneration = new NetworkVariable<float>(); //суммарная генерация энергии со всех модулей
    public NetworkVariable<float> energyMaxCapacity = new NetworkVariable<float>(); //максимальное количество запасаемой энергии во всех модулях
    public NetworkVariable<float> energy = new NetworkVariable<float>(); //текущее количество энергии в батареях
    public NetworkVariable<float> enginesConsumption = new NetworkVariable<float>(); //макс. потребление двигателями при полёте
    public float accelerationPower; //общая ускорительная мощь двигателей
    public float angularAccelerationPower; //общая вращательная мощь двигателей

    ShipStats myShipStats;
    ItemData myItemData;
    EnergyBar energyBar;
    Rigidbody2D myRigidbody2D;
    [SerializeField] NetworkVariable<bool> noEnergy = new NetworkVariable<bool>();

    //данные с джойстика игрока
    [HideInInspector] public NetworkVariable<bool> movementJoystickPressed = new NetworkVariable<bool>();
    [HideInInspector] public NetworkVariable<float> movementJoystickDirInDegrees = new NetworkVariable<float>();
    [HideInInspector] public NetworkVariable<float> movementJoystickMagnitude = new NetworkVariable<float>();

    //модификаторы полёта
    const float ignoredDirDifferenceDegrees = 60;
    const float minDrag = 0;
    const float maxSpeedMod = 150;
    const float linearDragMod = 0.01f;
    const float accelerationPowerMod = 1000;
    const float rotationForceMod = 100000;

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
        StartCoroutine(WaitingForChangingImageToDestroyed());
    }

    IEnumerator WaitingForChangingImageToDestroyed()
    {
        while (true)
        {
            if (alreadyDestroyed.Value)
            {
                shipsSpriteRenderer.sprite = destroyedImage;
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

        mass = myShipStats.totalMass;

        myRigidbody2D = GetComponent<Rigidbody2D>();
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
            if (energy.Value + (energyGeneration.Value * Time.fixedDeltaTime) >= energyMaxCapacity.Value)
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

    void FlightEffectsServer()
    {
        float enginesLightsAlphaFrameChange = enginesLightsAlphaChangingSpeed * Time.fixedDeltaTime;

        if (accelerationPower > 0.01f && !noControl && movementJoystickPressed.Value && Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, movementJoystickDirInDegrees.Value)) < ignoredDirDifferenceDegrees)
        {
            float enginesLightsMod;

            if (CheckEnergy(((enginesConsumption.Value * movementJoystickMagnitude.Value) + energyGeneration.Value) * Time.fixedDeltaTime))
            {
                enginesLightsMod = movementJoystickMagnitude.Value;
            }
            else
            {
                enginesLightsMod = movementJoystickMagnitude.Value * (energyGeneration.Value / (enginesConsumption.Value * movementJoystickMagnitude.Value));
            }

            if (CheckEnergy(enginesConsumption.Value * movementJoystickMagnitude.Value * Time.fixedDeltaTime))
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
        if (energy.Value <= energyGeneration.Value * Time.fixedDeltaTime || energyMaxCapacity.Value < 0.01f)
        {
            noEnergy.Value = true;
        }
        else
        {
            noEnergy.Value = false;
        }
        if (energy.Value < energyMaxCapacity.Value)
        {
            energy.Value += energyGeneration.Value * Time.fixedDeltaTime;
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

    public bool TrySpendEnergy(float amount)
    {
        if (amount <= 0)
            return false;

        if (energy.Value < amount)
            return false;

        energy.Value -= amount;

        return true;
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
        float F = angularAccelerationPower * rotationForceMod; //крутящий момент (аналог силы)
        float m = myRigidbody2D.inertia; //момент инерции (аналог массы)

        float v2 = Mathf.Abs(Mathf.Pow(myRigidbody2D.angularVelocity, 2)); //угловая скорость в квадрате
        if (myRigidbody2D.angularVelocity < 0)
        {
            v2 *= -1;
        }

        float a = F / m; //угловое ускорение
        float S = Mathf.DeltaAngle(transform.eulerAngles.z, movementJoystickDirInDegrees.Value);

        //Debug.Log($"S: {S}; v2: {v2}; a: {a}; 2as: {2 * a * S}");

        float ignoredDir = a * Mathf.Pow(Time.fixedDeltaTime * 2, 2);

        if (Mathf.Abs(S) < ignoredDir && Mathf.Abs(myRigidbody2D.angularVelocity) < a / Time.fixedDeltaTime / 2)
        {
            transform.eulerAngles = new Vector3(0, 0, movementJoystickDirInDegrees.Value);
            myRigidbody2D.angularVelocity = 0;
        }
        else
        {
            if (TrySpendEnergy(enginesConsumption.Value * Time.fixedDeltaTime))
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
                if (TrySpendEnergy(enginesConsumption.Value * movementJoystickMagnitude.Value * Time.fixedDeltaTime))
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


    public void ReduceShipСharacteristics(Durability destroyedModule)
    {
        ItemData itemData = destroyedModule.GetComponent<ItemData>();
        Battery battery = destroyedModule.GetComponent<Battery>();
        EnergyGenerator energyGenerator = destroyedModule.GetComponent<EnergyGenerator>();
        Engine engine = destroyedModule.GetComponent<Engine>();

        if (itemData != null)
        {
            mass -= itemData.Mass;
            myRigidbody2D.mass = mass;
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
            energy.Value -= (energy.Value / energyMaxCapacity.Value) * battery.maxCapacity;
            energyMaxCapacity.Value -= battery.maxCapacity;
        }
        if (energyGenerator != null)
        {
            energyGeneration.Value -= energyGenerator.power;
        }
        if (engine != null)
        {
            enginesConsumption.Value -= engine.powerConsumption;
            accelerationPower -= engine.accelerationPower;
            angularAccelerationPower -= engine.angularPower;
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
            float maxForce = forceOnDestroy * mass;
            Vector2 force = new Vector2(Random.Range(-maxForce, maxForce), Random.Range(-maxForce, maxForce));
            myRigidbody2D.AddForce(force);

            float maxRotationForce = rotationForceOnDestroy * mass;
            float rotationForce = Random.Range(-maxRotationForce, maxRotationForce);
            myRigidbody2D.AddTorque(rotationForce);

            ShowDestroyEffectsRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    void ShowDestroyEffectsRpc()
    {
        foreach (Effect effect in destroyEffects)
        {
            List<GameObject> spawnedGOs = effect.SpawnEffectsFromPool(Vector3.zero, Quaternion.identity);
            foreach (GameObject spawnedGO in spawnedGOs)
            {
                spawnedGO.transform.parent = transform;
                spawnedGO.transform.localPosition = Vector3.zero;
                spawnedGO.transform.localRotation = Quaternion.identity;
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
