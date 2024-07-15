using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ShipGameStats : NetworkBehaviour
{
    [Header("Настройка")]
    [SerializeField] private float _enginesVisualPowerChangingSpeed = 2;
    [SerializeField] private List<TrailRenderer> _trails;
    [SerializeField] private List<SpriteRenderer> _enginesLights;
    [SerializeField] private AudioSource _enginesAudioSource;
    [SerializeField] private float _engineSoundVolumeMod = 1;

    [SerializeField] private List<Effect> _destroyEffects;
    [SerializeField] private Sprite _destroyedImage;
    [Tooltip("Импульс при уничтожении корабля")]
    [SerializeField] private float _forceOnDestroy;
    [Tooltip("Вращательный импульс при уничтожении корабля")]
    [SerializeField] private float _rotationForceOnDestroy;

    [Header("Отладка")]
    public NetworkVariable<NetworkString> TeamID = new(new());
    [HideInInspector] public Durability ControlBlock;
    public float Mass; //масса корпуса с модулями
    public NetworkVariable<float> EnergyGeneration = new(); //суммарная генерация энергии со всех модулей
    public NetworkVariable<float> EnergyMaxCapacity = new(); //максимальное количество запасаемой энергии во всех модулях
    public NetworkVariable<float> Energy = new(); //текущее количество энергии в батареях
    public NetworkVariable<float> EnginesConsumption = new(); //макс. потребление двигателями при полёте
    public float AccelerationPower; //общая ускорительная мощь двигателей
    public float AngularAccelerationPower; //общая вращательная мощь двигателей

    private ShipStats _myShipStats;
    private ItemData _myItemData;
    private EnergyBar _energyBar;
    private Rigidbody2D _myRigidbody2D;
    [SerializeField] private NetworkVariable<bool> _noEnergy = new();

    //данные с джойстика игрока
    [HideInInspector] public NetworkVariable<bool> MovementJoystickPressed = new NetworkVariable<bool>();
    [HideInInspector] public NetworkVariable<float> MovementJoystickDirInDegrees = new NetworkVariable<float>();
    [HideInInspector] public NetworkVariable<float> MovementJoystickMagnitude = new NetworkVariable<float>();

    //модификаторы полёта
    private const float IgnoredDirDifferenceDegrees = 60;
    private const float MinDrag = 0;
    private const float MaxSpeedMod = 200;
    private const float LinearDragMod = 0.3f;
    private const float AccelerationPowerMod = 1600;
    private const float RotationForceMod = 75000;

    private const float MinJoystickMagnitudeToAccelerate = 0.7f;

    private bool _noControl; //true когда нету блока управления
    public NetworkVariable<bool> Destroyed = new NetworkVariable<bool>();
    private SpriteRenderer _shipsSpriteRenderer;

    [HideInInspector] public Player MyPlayer;

    private NetworkVariable<float> _enginesVisualPowerMod = new NetworkVariable<float>();
    private NetworkVariable<bool> _trailsEmitting = new NetworkVariable<bool>();

    private const float TimeToDisappearAfterDestroy = 60;

    private void Start()
    {
        _myItemData = GetComponent<ItemData>();

        if (!DataOperator.gameScene)
        {
            return;
        }
        if (IsOwner)
        {
            _energyBar = ShipInterfaceManager.Instance.energyBar;
            CameraMover.instance.SetPlayerShip(transform);
        }
        foreach (SpriteRenderer engineLight in _enginesLights)
        {
            engineLight.gameObject.SetActive(true);
        }
        _shipsSpriteRenderer = _myItemData.Image.GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        StartCoroutine(WaitingForBeingDestroyed());
    }

    IEnumerator WaitingForBeingDestroyed()
    {
        while (true)
        {
            if (Destroyed.Value)
            {
                DisableFlightEffects();
                _shipsSpriteRenderer.sprite = _destroyedImage;
                enabled = false;
                break;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    public void ServerInitialize()
    {
        _myShipStats = GetComponent<ShipStats>();

        Mass = _myShipStats.totalMass;

        _myRigidbody2D = GetComponent<Rigidbody2D>();
        _myRigidbody2D.mass = Mass;

        EnergyGeneration.Value = _myShipStats.totalEnergyGeneration;
        EnergyMaxCapacity.Value = _myShipStats.totalEnergyCapacity;
        Energy.Value = EnergyMaxCapacity.Value;

        EnginesConsumption.Value = _myShipStats.totalEnginesConsumption;
        AccelerationPower = _myShipStats.totalAccelerationPower;
        AngularAccelerationPower = _myShipStats.totalAngularAccelerationPower;

        Invoke(nameof(WaitingToBeSpawned), 0.1f);
    }

    void WaitingToBeSpawned()
    {
        if (IsSpawned)
        {
            OwnerInitializeRpc();
            if (_myShipStats.ControlBlockExists())
            {
                ChangeOwnersInterfaceStateRpc(true);
            }
            else
            {
                _noControl = true;
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
                if (MyPlayer == null)
                {
                    //игрок, управляющий кораблём, отключился
                    ExplodeTheShipOnServer();
                    return;
                }
                if (!_noControl)
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
                    _myRigidbody2D.drag = MinDrag;
                }
                GenerateEnergy();
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
        if (_noEnergy.Value)
        {
            _energyBar.fillingValue = 0;
        }
        else
        {
            if (Energy.Value + (EnergyGeneration.Value * Time.deltaTime) >= EnergyMaxCapacity.Value)
            {
                _energyBar.fillingValue = 1;
            }
            else
            {
                if (EnergyMaxCapacity.Value > 0.001f)
                {
                    _energyBar.fillingValue = Energy.Value / EnergyMaxCapacity.Value;
                }
                else
                {
                    _energyBar.fillingValue = 0;
                }
            }
        }
    }

    void FlightEffectsServer(bool accelerating)
    {
        float enginesLightsAlphaFrameChange = _enginesVisualPowerChangingSpeed * Time.deltaTime;

        if (AccelerationPower > 0.01f && !_noControl && accelerating && Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, MovementJoystickDirInDegrees.Value)) < IgnoredDirDifferenceDegrees)
        {
            float enginesLightsMod;

            if (CheckEnergy(((EnginesConsumption.Value * MovementJoystickMagnitude.Value) + EnergyGeneration.Value) * Time.deltaTime))
            {
                enginesLightsMod = MovementJoystickMagnitude.Value;
            }
            else
            {
                enginesLightsMod = MovementJoystickMagnitude.Value * (EnergyGeneration.Value / (EnginesConsumption.Value * MovementJoystickMagnitude.Value));
            }

            if (CheckEnergy(EnginesConsumption.Value * MovementJoystickMagnitude.Value * Time.deltaTime))
            {
                _trailsEmitting.Value = true;
            }
            else
            {
                _trailsEmitting.Value = false;
            }

            if (_enginesVisualPowerMod.Value < enginesLightsMod)
            {
                _enginesVisualPowerMod.Value += enginesLightsAlphaFrameChange;
            }
            if (_enginesVisualPowerMod.Value > enginesLightsMod)
            {
                _enginesVisualPowerMod.Value -= enginesLightsAlphaFrameChange;
            }
            if (Mathf.Abs(_enginesVisualPowerMod.Value - enginesLightsMod) < enginesLightsAlphaFrameChange)
            {
                _enginesVisualPowerMod.Value = enginesLightsMod;
            }
        }
        else
        {
            _trailsEmitting.Value = false;
            if (_enginesVisualPowerMod.Value > 0)
            {
                _enginesVisualPowerMod.Value -= enginesLightsAlphaFrameChange;
            }
        }
    }

    void FlightEffectsClient()
    {
        SetEnginesVisualPower(_enginesVisualPowerMod.Value);

        if (_trailsEmitting.Value)
        {
            foreach (TrailRenderer trail in _trails)
            {
                trail.emitting = true;
            }
        }
        else
        {
            foreach (TrailRenderer trail in _trails)
            {
                trail.emitting = false;
            }
        }
    }



    void GenerateEnergy()
    {
        if (Energy.Value <= EnergyGeneration.Value * Time.deltaTime || EnergyMaxCapacity.Value < 0.01f)
        {
            _noEnergy.Value = true;
        }
        else
        {
            _noEnergy.Value = false;
        }
        if (Energy.Value < EnergyMaxCapacity.Value)
        {
            Energy.Value += EnergyGeneration.Value * Time.deltaTime;
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
        //тут жоская физика
        float F = AngularAccelerationPower * RotationForceMod; //крутящий момент (аналог силы)
        float m = _myRigidbody2D.inertia; //момент инерции (аналог массы для вращательного движения)

        float v2 = Mathf.Abs(Mathf.Pow(_myRigidbody2D.angularVelocity, 2)); //угловая скорость в квадрате
        if (_myRigidbody2D.angularVelocity < 0)
        {
            v2 *= -1;
        }

        float a = F / m; //угловое ускорение
        float S = Mathf.DeltaAngle(transform.eulerAngles.z, MovementJoystickDirInDegrees.Value);

        //Debug.Log($"S: {S}; v2: {v2}; a: {a}; 2as: {2 * a * S}");

        float ignoredDir = a * Mathf.Pow(Time.deltaTime * 2, 2) * 2;

        if (Mathf.Abs(S) < ignoredDir && Mathf.Abs(_myRigidbody2D.angularVelocity) < a * 0.2f)
        {
            transform.eulerAngles = new Vector3(0, 0, MovementJoystickDirInDegrees.Value);
            _myRigidbody2D.angularVelocity = 0;
        }
        else
        {
            if (TrySpendEnergy(EnginesConsumption.Value * 0.3f * Time.deltaTime))
            {
                if (v2 * 1.2f < 2 * a * S) //ещё не разогнались достаточно, продолжаем ускоряться
                {
                    //Debug.Log("S < 0, ускоряемся");
                    _myRigidbody2D.AddTorque(F * Time.deltaTime);
                }
                else //тормозим дабы не возникло колебаний
                {
                    //Debug.Log("S < 0, тормозим");
                    _myRigidbody2D.AddTorque(-F * Time.deltaTime * 1.2f);
                }
            }
        }
    }

    void Accelerate()
    {
        bool accelerating = MovementJoystickMagnitude.Value > MinJoystickMagnitudeToAccelerate && MovementJoystickPressed.Value;
        FlightEffectsServer(accelerating);

        if (accelerating)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, MovementJoystickDirInDegrees.Value)) < IgnoredDirDifferenceDegrees)
            {
                if (TrySpendEnergy(EnginesConsumption.Value * MovementJoystickMagnitude.Value * Time.deltaTime))
                {
                    _myRigidbody2D.AddForce(DataOperator.RotateVector2(new Vector2(0, AccelerationPower * AccelerationPowerMod * MovementJoystickMagnitude.Value), MovementJoystickDirInDegrees.Value));
                }
            }
        }
        else
        {
            _myRigidbody2D.drag = MinDrag;
        }
    }

    void ApplyFriction()
    {
        Vector3 velocity = _myRigidbody2D.velocity;
        float maxSpeed = Mathf.Pow(AccelerationPower / Mass, 1f / 2) * MaxSpeedMod;

        if (velocity.magnitude > maxSpeed)
        {
            _myRigidbody2D.drag = MinDrag + ((velocity.magnitude - maxSpeed) * LinearDragMod);
        }
        else
        {
            _myRigidbody2D.drag = MinDrag;
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
            _myRigidbody2D.mass = Mass;
            if (!Destroyed.Value && itemData.Type == modulesTypes.ControlModules)
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
        if (!Destroyed.Value)
        {
            //отключаем все ещё оставшиеся системы корабля
            enabled = false;
            _myRigidbody2D.drag = MinDrag;
            if (MyPlayer != null)
            {
                DisconnectFromTheShip();
            }
            _noControl = true;
            Weapon[] weapons = GetComponentsInChildren<Weapon>();
            foreach (Weapon weapon in weapons)
            {
                weapon.Disconnect();
            }

            Destroyed.Value = true;
            float maxForce = _forceOnDestroy * Mass;
            Vector2 force = new Vector2(Random.Range(-maxForce, maxForce), Random.Range(-maxForce, maxForce));
            _myRigidbody2D.AddForce(force);

            float maxRotationForce = _rotationForceOnDestroy * Mass;
            float rotationForce = Random.Range(-maxRotationForce, maxRotationForce);
            _myRigidbody2D.AddTorque(rotationForce);

            DisableFlightEffectsRpc();
            ShowDestroyEffectsRpc(_myRigidbody2D.velocity);

            Invoke(nameof(DissapearRpc), TimeToDisappearAfterDestroy);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void DissapearRpc()
    {
        StartCoroutine(DissapearingEffect());
    }

    private IEnumerator DissapearingEffect()
    {
        while (_shipsSpriteRenderer.color.a > 0)
        {
            Color oldColor = _shipsSpriteRenderer.color;
            _shipsSpriteRenderer.color = new Color(oldColor.r, oldColor.g, oldColor.b, oldColor.a - Time.deltaTime);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        if (NetworkManager.Singleton.IsServer)
        {
            Destroy(gameObject);
        }
    }

    void DisableFlightEffects()
    {
        SetEnginesVisualPower(0);
        foreach (TrailRenderer trail in _trails)
        {
            trail.emitting = false;
        }
    }

    void SetEnginesVisualPower(float visualPowerMod)
    {
        foreach (SpriteRenderer engineLight in _enginesLights)
        {
            Color oldColor = engineLight.color;
            engineLight.color = new Color(oldColor.r, oldColor.g, oldColor.b, visualPowerMod);
        }
        _enginesAudioSource.volume = visualPowerMod * _engineSoundVolumeMod;
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
        foreach (Effect effect in _destroyEffects)
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
        if (_myItemData == null)
        {
            _myItemData = GetComponent<ItemData>();
        }
        Vector3 extremePoints = _myItemData.GetMaxSlotsPosition() - _myItemData.GetMinSlotsPosition();
        float shipSize = extremePoints.magnitude;
        GameCameraScaler.instance.SetCameraLimits(shipSize);

        ShipInterfaceManager.Instance.SetLocalPlayerToRadar(this);
    }

    [Rpc(SendTo.Owner)]
    void SendMessageToOwnerRpc(TranslatedNetworkText networkMessage)
    {
        TranslatedText message = networkMessage.GetTranslatedText();
        ShipInterfaceManager.Instance.ShowWarningText(message);
    }

    [Rpc(SendTo.Owner)]
    void ChangeOwnersInterfaceStateRpc(bool enableShipInterface)
    {
        if (enableShipInterface)
        {
            GameInterfaceManager.Instance.EnableShipInterface();
        }
        else
        {
            GameInterfaceManager.Instance.DisableAllInterfaces();
        }
    }


    public delegate void AttackButtonStateChangedMessage(uint index, bool pressed);
    public event AttackButtonStateChangedMessage attackButtonStateChangedMessage;

    public void SendFireStateChange(uint index, bool fire)
    {
        attackButtonStateChangedMessage?.Invoke(index, fire);
    }
}
