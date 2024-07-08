using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ModulesCellsDurabilityShower : NetworkBehaviour
{
    [Header("Настройка")]
    [SerializeField] private Transform healthCellsParent;
    [SerializeField] private GameObject healthCellPrefab;
    [SerializeField] private Sprite destroyedHealthCellSprite;

    public float maxCameraZoomToShowHealthCells;
    
    [Tooltip("Скорость изменения прозрачности ячеек прочности когда они становяться видимыми/невидимыми из-за приближения/отдаления камеры")]
    [SerializeField] float healthCellsAlphaChangingSpeedWhileVisibilityChanging = 5;
    [Tooltip("Скорость изменения прозрачности ячеек прочности когда они становяться видимыми/невидимыми из-за приближения/отдаления камеры")]
    [SerializeField] float healthCellsAlphaChangingSpeedWhenDurabilityChanged = 25;

    private readonly Dictionary<LowAccuracyVector2, HealthCell> _healthCellsSpawned = new Dictionary<LowAccuracyVector2, HealthCell>();
    private float _transparencyControlledByCamera;
    private float _maxCellAlpha;

    private bool _showHealthCells; 

    private void Start()
    {
        if (DataOperator.gameScene == false)
        {
            enabled = false;
            return;
        }
        _maxCellAlpha = healthCellPrefab.GetComponent<SpriteRenderer>().color.a;
        Update();
    }

    private void Update()
    {
        ControlCellsTransparency();
        SetCellsTransparency();
    }

    void ControlCellsTransparency()
    {
        ControlGlobalTransparencyOfTheHealthCells();
        ControlHealthCellsTransparencyOnDurabilityChanged();
    }

    void ControlGlobalTransparencyOfTheHealthCells()
    {
        //контроль глобальной видимости ячеек
        if (GameSettingsKeeper.instance.healthCellsShowPreset != HealthCellsShowPreset.never &&
            (GameSettingsKeeper.instance.healthCellsShowPreset == HealthCellsShowPreset.always ||
            (GameSettingsKeeper.instance.healthCellsShowPreset == HealthCellsShowPreset.onlyWhenZoomedIn &&
            GameCameraScaler.instance.showHealthCells)))
        {
            _showHealthCells = true;
        }
        else
        {
            _showHealthCells = false;
        }

        if (_showHealthCells)
        {
            if (_transparencyControlledByCamera < 1)
            {
                _transparencyControlledByCamera += healthCellsAlphaChangingSpeedWhileVisibilityChanging * Time.deltaTime;
            }
        }
        else
        {
            if (_transparencyControlledByCamera > 0)
            {
                _transparencyControlledByCamera -= healthCellsAlphaChangingSpeedWhileVisibilityChanging * Time.deltaTime;
            }
        }
    }

    void ControlHealthCellsTransparencyOnDurabilityChanged()
    {
        //контроль видимости некоторых ячеек из-за изменения их прочности
        float alphaChangingSpeed = healthCellsAlphaChangingSpeedWhenDurabilityChanged * Time.fixedDeltaTime;
        float healthCellsShowTimeDurabilityChanged = GameSettingsKeeper.instance.healthCellsShowTimeOnDurabilityChanged;
        foreach (var helthCellWithKey in _healthCellsSpawned)
        {
            HealthCell healthCell = helthCellWithKey.Value;
            if (!healthCell.destroyed)
            {
                if (!_showHealthCells)
                {
                    if (healthCellsShowTimeDurabilityChanged > 0.001f)
                    {
                        switch (healthCell.healthCellState)
                        {
                            case HealthCellState.showingUp:
                                if (healthCell.damageAlpha < 1)
                                {
                                    healthCell.damageAlpha += alphaChangingSpeed * 2;
                                }
                                else
                                {
                                    healthCell.healthCellState = HealthCellState.waiting;
                                }

                                break;

                            case HealthCellState.waiting:
                                if (healthCell.visibilityTimer < healthCellsShowTimeDurabilityChanged)
                                {
                                    healthCell.visibilityTimer += Time.deltaTime;
                                }
                                else
                                {
                                    healthCell.healthCellState = HealthCellState.showingDown;
                                }

                                break;

                            case HealthCellState.showingDown:
                                if (healthCell.damageAlpha > 0)
                                {
                                    healthCell.damageAlpha -= alphaChangingSpeed;
                                }
                                else
                                {
                                    healthCell.healthCellState = HealthCellState.normal;
                                }

                                break;
                        }
                    }
                }
                else
                {
                    switch (healthCell.healthCellState)
                    {
                        case HealthCellState.showingUp:
                            if (healthCell.damageAlpha < 1)
                            {
                                healthCell.damageAlpha += alphaChangingSpeed * 2;
                            }
                            else
                            {
                                healthCell.healthCellState = HealthCellState.showingDown;
                            }

                            break;

                        case HealthCellState.waiting:
                            healthCell.healthCellState = HealthCellState.showingDown;
                            break;

                        case HealthCellState.showingDown:
                            if (healthCell.damageAlpha > 0)
                            {
                                healthCell.damageAlpha -= alphaChangingSpeed;
                            }
                            else
                            {
                                healthCell.healthCellState = HealthCellState.normal;
                            }

                            break;
                    }
                }
            }
            else
            {
                healthCell.damageAlpha = 0;
                healthCell.spriteRenderer.sprite = destroyedHealthCellSprite;

                if (healthCell.destroyedCurrentBlinkCount < HealthCell.destroyedBlinkCount)
                {
                    switch (healthCell.destroyedCellState)
                    {
                        case HealthCellState.normal:
                            healthCell.destroyedCellState = HealthCellState.showingUp;
                            break;

                        case HealthCellState.showingUp:
                            if (healthCell.destroyedAlpha < 1)
                            {
                                healthCell.destroyedAlpha += alphaChangingSpeed;
                            }
                            else
                            {
                                healthCell.destroyedCellState = HealthCellState.showingDown;
                                healthCell.destroyedCurrentBlinkCount++;
                            }
                            break;

                        case HealthCellState.showingDown:
                            if (healthCell.destroyedAlpha > 0)
                            {
                                healthCell.destroyedAlpha -= alphaChangingSpeed;
                            }
                            else
                            {
                                healthCell.destroyedCellState = HealthCellState.showingUp;
                            }
                            break;
                    }
                }
                else
                {
                    if (healthCell.destroyedTimerToShowDown < GameSettingsKeeper.instance.healthCellsShowTimeOnDurabilityChanged)
                    {
                        healthCell.destroyedTimerToShowDown += Time.deltaTime;
                    }
                    else
                    {
                        if (healthCell.destroyedAlpha > 0)
                        {
                            healthCell.destroyedAlpha -= alphaChangingSpeed;
                        }
                    }
                }
            }
        }
    }

    void SetCellsTransparency()
    {
        foreach (var helthCellWithKey in _healthCellsSpawned)
        {
            SetCellTransparency(helthCellWithKey.Value);
        }
    }

    void SetCellTransparency(HealthCell healthCell)
    {
        SpriteRenderer spriteRenderer = healthCell.spriteRenderer;
        Color oldColor = spriteRenderer.color;
        float newAlpha;

        if (GameCameraScaler.instance.zoom > GameCameraScaler.instance.peakMaxZoomToShowHealthCells ||
            GameSettingsKeeper.instance.healthCellsShowPreset == HealthCellsShowPreset.never)
        {
            newAlpha = 0;
        }
        else
        {
            if (_showHealthCells)
            {
                if (!healthCell.destroyed)
                {
                    newAlpha = _transparencyControlledByCamera * _maxCellAlpha * (1 - healthCell.damageAlpha);
                }
                else
                {
                    if (healthCell.destroyedTimerToShowDown < GameSettingsKeeper.instance.healthCellsShowTimeOnDurabilityChanged)
                    {
                        newAlpha = healthCell.destroyedAlpha * _maxCellAlpha;
                    }
                    else
                    {
                        newAlpha = _transparencyControlledByCamera * _maxCellAlpha;
                    }
                }
            }
            else
            {
                newAlpha = _maxCellAlpha * (healthCell.damageAlpha + healthCell.destroyedAlpha + _transparencyControlledByCamera);
            }
        }
        if (newAlpha > _maxCellAlpha)
        {
            newAlpha = _maxCellAlpha;
        }

        spriteRenderer.color = new Color(oldColor.r, oldColor.g, oldColor.b, newAlpha);
    }

    public void RenderHealthCells()
    {
        ItemData[] modulesDatas = GetComponentsInChildren<ItemData>();
        foreach (ItemData moduleData in modulesDatas)
        {
            if (moduleData.IsModule)
            {
                foreach (CellData cellData in moduleData.ItemCellsData)
                {
                    RenderHealthCellRpc((Vector2)moduleData.transform.localPosition + moduleData.CellsOffset + cellData.position);
                }
            }
        }
    }

    [Rpc(SendTo.Owner)]
    public void OnHealthCellDurabilityChangedRpc(float durabilityToMaxDurabilityRatio, Vector2Serializable[] healthCellsPositionsSerializable)
    {
        foreach (Vector2Serializable healthCellPositionSerializable in healthCellsPositionsSerializable)
        {
            HealthCell healthCellSpriteRenderer;
            LowAccuracyVector2 lowAccuracyCellPosition = new LowAccuracyVector2(healthCellPositionSerializable.GetVector2());
            if (_healthCellsSpawned.TryGetValue(lowAccuracyCellPosition, out healthCellSpriteRenderer))
            {
                RepaintDurabilityCell(healthCellSpriteRenderer, durabilityToMaxDurabilityRatio);
            }
            else
            {
                Debug.LogError($"ModulesCellsDurabilityShower: попытка изменить цвет не существующей ячейки, расположенной в {gameObject.name} и находящейся на localPosition {lowAccuracyCellPosition.GetVector2()} (но ячеек на этой позиции не зарегестрировано)");
            }
        }
    }

    void RepaintDurabilityCell(HealthCell healthCell, float durabilityToMaxDurabilityRatio)
    {
        SpriteRenderer healthCellsSpriteRenderer = healthCell.spriteRenderer;
        float redRatio = 0;
        float greenRatio = 0;
        float blueRatio = 0;
        float cellTransparency = healthCellsSpriteRenderer.color.a;

        if (durabilityToMaxDurabilityRatio > 0.5f)
        {
            redRatio = (1f - durabilityToMaxDurabilityRatio) * 2;
            greenRatio = 1;
        }
        if (durabilityToMaxDurabilityRatio == 0.5f)
        {
            redRatio = 1;
            greenRatio = 1;
        }
        if (durabilityToMaxDurabilityRatio < 0.5f)
        {
            redRatio = 1;
            greenRatio = durabilityToMaxDurabilityRatio * 2;
        }

        Color newColor = new Color(redRatio, greenRatio, blueRatio, cellTransparency);
        healthCellsSpriteRenderer.color = newColor;
        if (GameSettingsKeeper.instance.healthCellsShowPreset != HealthCellsShowPreset.never)
        {
            if (durabilityToMaxDurabilityRatio > 0.001f)
            {
                healthCell.OnDurabilityChanged();
            }
            else
            {
                healthCell.destroyed = true;
                healthCell.spriteRenderer.color = new Color(1, 1, 1, healthCell.spriteRenderer.color.a);
            }
        }
    }

    [Rpc(SendTo.Owner)]
    void RenderHealthCellRpc(Vector2 localPosition)
    {
        GameObject newHealthCellSpawned = Instantiate(healthCellPrefab, Vector3.zero, Quaternion.identity);

        newHealthCellSpawned.transform.parent = healthCellsParent;
        newHealthCellSpawned.transform.localPosition = localPosition;
        newHealthCellSpawned.transform.rotation = Quaternion.identity;

        _healthCellsSpawned.Add(new LowAccuracyVector2(localPosition), new HealthCell(newHealthCellSpawned.GetComponent<SpriteRenderer>()));
    }

    class HealthCell
    {
        public SpriteRenderer spriteRenderer;
        public float damageAlpha = 0; //модификатор прозрачности при изменении прочности
        public bool destroyed = false;

        public float visibilityTimer;
        public HealthCellState healthCellState = HealthCellState.normal;

        //уничтоженные ячейки
        public const int destroyedBlinkCount = 5;
        public float destroyedAlpha = 0;
        public float destroyedCurrentBlinkCount = 0;
        public float destroyedTimerToShowDown;
        public HealthCellState destroyedCellState = HealthCellState.normal;

        public HealthCell(SpriteRenderer spriteRenderer_)
        {
            spriteRenderer = spriteRenderer_;
        }

        public void OnDurabilityChanged()
        {
            healthCellState = HealthCellState.showingUp;
            visibilityTimer = 0;
        }
    }

    enum HealthCellState
    {
        normal,
        showingUp,
        waiting,
        showingDown
    }
}

public struct LowAccuracyVector2
{
    readonly long xCompressed; //домноженный на 100 и округлённый до целого X
    readonly long yCompressed; //домноженный на 100 и округлённый до целого Y
    const int accuracy = 100; //точность (больше == выше)

    public LowAccuracyVector2(Vector2 _vector2)
    {
        xCompressed = Mathf.RoundToInt(_vector2.x * accuracy);
        yCompressed = Mathf.RoundToInt(_vector2.y * accuracy);
    }

    public Vector2 GetVector2()
    {
        return new Vector2((float)xCompressed / accuracy, (float)yCompressed / accuracy);
    }
}