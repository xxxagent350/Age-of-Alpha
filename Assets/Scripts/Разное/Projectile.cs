using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Настройка")]
    [Tooltip("Урон")]
    public Damage Damage;
    [Tooltip("Позиция выхода луча для поиска целей, расположить чуть выше коллайдера снаряда")]
    [SerializeField] private Vector2 _raycastPosition;
    [Tooltip("Стартовая скорость")]
    public float StartSpeed;
    [Tooltip("Масса. От неё зависит импульс при выстреле и попадании")]
    public float Mass;
    [Tooltip("Максимальное время существования снаряда")]
    public float Lifetime;
    [Tooltip("Обязательно ли снаряд уничтожится после первого попадания? Если вЫключено, снаряд будет лететь, попутно разрушая модули и вражеские снаряды, пока у него не кончится урон")]
    [SerializeField] private bool _selfDestructAfterHit;
    [Tooltip("Взрываться ли когда заканчивается время жизни снаряда? Если выключено, снаряд просто пропадёт")]
    [SerializeField] private bool _explodeOnLifetimeEnds = false;
    [Tooltip("Если включено, будет поворачивать картинку в сторону движения")]
    [SerializeField] private bool _rotateToVelocityVectorDir = false;

    [Tooltip("Эффекты пробития обшивки корабля (укажите названия эффектов из префаба RpcHandlerForEffects)")]
    [SerializeField] private List<string> _shipPenetrationEffects;
    [Tooltip("Эффекты попадания в модули корабля, снаряды, астероиды и прочие игровые объекты (укажите названия эффектов из префаба RpcHandlerForEffects)")]
    [SerializeField] private List<string> _moduleHitEffects;
    [Tooltip("Эффекты взрыва снаряда (укажите названия эффектов из префаба RpcHandlerForEffects)")]
    [SerializeField] private List<string> _explodeEffects;

    [Header("Отладка")]
    [Tooltip("Команда корабля, который выпустил снаряд")]
    public string teamID = "none";

    private Rigidbody2D _myRigidbody2D;
    private bool _insideEnemyShip;
    private float _startFullDamage;

    [Tooltip("Последняя точка, в которой был нанесён урон (нужно для определения положения эффекта взрыва)")]
    private Vector3 _lastHitPoint;
    [Tooltip("Скорость последнего объекта, в который врезался снаряд (нужно для скорости эффектов при взрыве снаряда)")]
    private Vector3 _lastHitCollidersSpeed = Vector3.one;

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (_explodeOnLifetimeEnds)
            {
                Invoke(nameof(Explode), Lifetime);
            }
            else
            {
                Destroy(gameObject, Lifetime);
            }

            _startFullDamage = Damage.GetAllDamage();
            _myRigidbody2D = GetComponent<Rigidbody2D>();
            _myRigidbody2D.velocity += DataOperator.RotateVector2(Vector2.up * StartSpeed, transform.eulerAngles.z);
        }
    }

    public void ApplyImpulseToParentShip(Rigidbody2D parentShipRigidbody)
    {
        //импульс стреляющему кораблю
        Vector2 force = DataOperator.RotateVector2(Vector2.down * StartSpeed * Mass, transform.eulerAngles.z);
        Vector2 position = transform.position;
        parentShipRigidbody.AddForceAtPosition(force, position);
    }

    public void ApplyImpulseToTargetShip(Rigidbody2D targetShipRigidbody, float damagePartUsed)
    {
        //импульс кораблю в которого попали
        if (targetShipRigidbody != null)
        {
            Vector2 force = _myRigidbody2D.velocity * damagePartUsed * Mass;
            Vector2 position = transform.position;
            targetShipRigidbody.AddForceAtPosition(force, position);
        }
    }

    private void FixedUpdate()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (_rotateToVelocityVectorDir)
            {
                SetDirAtMovingDir();
            }
            if (Damage.AllDamageUsed())
            {
                Explode();
            }
            RaycastAndGiveDamage();
        } 
    }

    private void RaycastAndGiveDamage()
    {
        if (alreadyExploded)
        {
            return;
        }
        float distanceToCheck = _myRigidbody2D.velocity.magnitude * Time.fixedDeltaTime;

        Vector3 position = transform.position + (Vector3)DataOperator.RotateVector2(_raycastPosition, transform.eulerAngles.z);
        Vector3 direction = _myRigidbody2D.velocity.normalized;

        RaycastHit2D[] mainLayerHits = Physics2D.RaycastAll(position, direction, distanceToCheck);

        if (mainLayerHits.Length > 0)
        {
            bool insideEnemyShipThisFrame = false;
            foreach (RaycastHit2D hitInfo in mainLayerHits)
            {
                if (hitInfo.collider != null)
                {
                    LayerMask hitInfoLayer = hitInfo.collider.gameObject.layer;

                    Rigidbody2D collidersRigidbody2D = hitInfo.collider.GetComponent<Rigidbody2D>();
                    Vector3 collidersSpeed = Vector3.zero;
                    if (collidersRigidbody2D != null)
                    {
                        collidersSpeed = collidersRigidbody2D.velocity;
                        _lastHitCollidersSpeed = collidersSpeed;
                    }

                    if (hitInfoLayer == LayerMask.NameToLayer("Ship"))
                    {
                        ShipGameStats shipGameStats = hitInfo.collider.GetComponent<ShipGameStats>();
                        if (shipGameStats != null && shipGameStats.TeamID.Value.GetString() != teamID)
                        {
                            if (!_insideEnemyShip)
                            {
                                SpawnEffect(EffectType.shipPenetration, hitInfo.point, transform.rotation, collidersSpeed);
                            }
                            _insideEnemyShip = true;
                            insideEnemyShipThisFrame = true;
                        }
                    }
                    else
                    {
                        if (hitInfoLayer == LayerMask.NameToLayer("Projectile"))
                        {
                            Projectile hittedProjectile = hitInfo.collider.GetComponent<Projectile>();
                            if (hittedProjectile != null && hittedProjectile.teamID != teamID)
                            {
                                SpawnEffect(EffectType.moduleHit, hitInfo.point, transform.rotation, collidersSpeed);
                                hittedProjectile.Damage.DamageOtherDamage(Damage);
                                _lastHitPoint = hitInfo.point;
                            }
                        }
                        if (hitInfoLayer == LayerMask.NameToLayer("Environment") || hitInfoLayer == LayerMask.NameToLayer("Module"))
                        {
                            Durability hittedObject = hitInfo.collider.GetComponent<Durability>();
                            if (hittedObject != null && hittedObject.teamID != teamID)
                            {
                                SpawnEffect(EffectType.moduleHit, hitInfo.point, transform.rotation, collidersSpeed);
                                float previousDamage = Damage.GetAllDamage();
                                hittedObject.durability.TakeDamage(Damage);
                                float deltaDamage = previousDamage - Damage.GetAllDamage();
                                ApplyImpulseToTargetShip(hitInfo.collider.GetComponentInParent<Rigidbody2D>(), deltaDamage / _startFullDamage);
                                _lastHitPoint = hitInfo.point;
                            }
                        }
                        if (_selfDestructAfterHit)
                        {
                            Explode();
                        }
                    }
                }
            }
            if (!insideEnemyShipThisFrame)
            {
                _insideEnemyShip = false;
            }
        }
        else
        {
            _insideEnemyShip = false;
        }
    }

    bool alreadyExploded = false;

    private void Explode()
    {
        if (!alreadyExploded)
        {
            if (_lastHitCollidersSpeed == Vector3.one)
            {
                SpawnEffect(EffectType.explode, _lastHitPoint, transform.rotation, _myRigidbody2D.velocity);
            }
            else
            {
                SpawnEffect(EffectType.explode, _lastHitPoint, transform.rotation, _lastHitCollidersSpeed);
            }
            alreadyExploded = true;
            Destroy(gameObject);
        }
    }

    void SetDirAtMovingDir()
    {
        transform.eulerAngles = new Vector3(0, 0, DataOperator.GetVector2DirInDegrees(_myRigidbody2D.velocity));
    }

    void SpawnEffect(EffectType effectType, Vector3 position, Quaternion rotation, Vector3 speed)
    {
        switch (effectType)
        {
            case EffectType.shipPenetration:
                RpcHandlerForEffects.SpawnEffectsOnClients(_shipPenetrationEffects, position, rotation, speed);
                break;
            case EffectType.moduleHit:
                RpcHandlerForEffects.SpawnEffectsOnClients(_moduleHitEffects, position, rotation, speed);
                break;
            case EffectType.explode:
                RpcHandlerForEffects.SpawnEffectsOnClients(_explodeEffects, position, rotation, speed);
                break;
        }
    }

    enum EffectType
    {
        shipPenetration,
        moduleHit,
        explode
    }

#if UNITY_EDITOR
    //отрисовка позиции выхода raycast
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            //Gizmos.color = Color.black;
            Vector2 myPosition = transform.position;
            Gizmos.DrawIcon(myPosition + _raycastPosition, "Raycast icon.png", false);
        }
    }
#endif
}
