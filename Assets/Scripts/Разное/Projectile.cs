using UnityEngine;
using Unity.Netcode;

public class Projectile : MonoBehaviour
{
    [Header("Настройка")]
    [Tooltip("Урон")]
    public Damage damage;
    [Tooltip("Позиция выхода луча для поиска целей, расположить чуть выше коллайдера снаряда")]
    [SerializeField] Vector2 raycastPosition;
    [Tooltip("Стартовая скорость")]
    public float startSpeed;
    [Tooltip("Максимальное время существования снаряда")]
    public float lifetime;
    [Tooltip("Тип урона")]
    [SerializeField] DamageTypes damageType;
    [Tooltip("Обязательно ли снаряд уничтожится после первого попадания? Если вЫключено, снаряд будет лететь, попутно разрушая модули и вражеские снаряды, пока у него не кончится урон")]
    [SerializeField] bool selfDestructAfterHit;
    [Tooltip("Взрываться ли когда заканчивается время жизни снаряда? Если выключено, снаряд просто пропадёт")]
    [SerializeField] bool explodeOnLifetimeEnds = false;
    [Tooltip("Если включено, будет поворачивать картинку в сторону движения")]
    [SerializeField] bool rotateToVelocityVectorDir = false;

    [Tooltip("Эффект пробития обшивки корабля (укажите название эффекта из префаба RpcHandlerForEffects)")]
    [SerializeField] string shipPenetrationEffects;
    [Tooltip("Эффект попадания в модули корабля, снаряды, астероиды и прочие игровые объекты (укажите название эффекта из префаба RpcHandlerForEffects)")]
    [SerializeField] string moduleHitEffects;
    [Tooltip("Эффект взрыва снаряда (укажите название эффекта из префаба RpcHandlerForEffects)")]
    [SerializeField] string explodeEffects;

    [Header("Отладка")]
    [Tooltip("Команда корабля, который выпустил снаряд")]
    public string teamID = "none";

    Rigidbody2D myRigidbody2D;
    bool insideEnemyShip;

    [Tooltip("Последняя точка, в которой был нанесён урон (нужно для определения положения эффекта взрыва)")]
    Vector3 lastHitPoint;

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (explodeOnLifetimeEnds)
            {
                Invoke(nameof(Explode), lifetime);
            }
            else
            {
                Destroy(gameObject, lifetime);
            }
            
            myRigidbody2D = GetComponent<Rigidbody2D>();
            myRigidbody2D.velocity += DataOperator.RotateVector2(new Vector2(0, startSpeed), transform.eulerAngles.z);
        }
    }

    private void FixedUpdate()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (rotateToVelocityVectorDir)
            {
                SetDirAtMovingDir();
            }
            if (damage.AllDamageUsed())
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
        float distanceToCheck = myRigidbody2D.velocity.magnitude * Time.fixedDeltaTime;

        Vector3 position = transform.position + (Vector3)DataOperator.RotateVector2(raycastPosition, transform.eulerAngles.z);
        Vector3 direction = myRigidbody2D.velocity.normalized;

        RaycastHit2D[] mainLayerHits = Physics2D.RaycastAll(position, direction, distanceToCheck);

        if (mainLayerHits.Length > 0)
        {
            bool insideEnemyShipThisFrame = false;
            foreach (RaycastHit2D hitInfo in mainLayerHits)
            {
                if (hitInfo.collider != null)
                {
                    LayerMask hitInfoLayer = hitInfo.collider.gameObject.layer;

                    if (hitInfoLayer == LayerMask.NameToLayer("Ship"))
                    {
                        ShipGameStats shipGameStats = hitInfo.collider.GetComponent<ShipGameStats>();
                        if (shipGameStats != null && shipGameStats.teamID != teamID)
                        {
                            if (!insideEnemyShip)
                            {
                                SpawnEffect(EffectType.shipPenetration, hitInfo.point, transform.rotation);
                            }
                            insideEnemyShip = true;
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
                                SpawnEffect(EffectType.moduleHit, hitInfo.point, transform.rotation);
                                hittedProjectile.damage.DamageOtherDamage(damage);
                                lastHitPoint = hitInfo.point;
                            }
                        }
                        if (hitInfoLayer == LayerMask.NameToLayer("Environment") || hitInfoLayer == LayerMask.NameToLayer("Module"))
                        {
                            Durability hittedObject = hitInfo.collider.GetComponent<Durability>();
                            if (hittedObject != null && hittedObject.teamID != teamID)
                            {
                                SpawnEffect(EffectType.moduleHit, hitInfo.point, transform.rotation);
                                hittedObject.durability.TakeDamage(damage);
                                lastHitPoint = hitInfo.point;
                            }
                        }
                        if (selfDestructAfterHit)
                        {
                            Explode();
                        }
                    }
                }
            }
            if (!insideEnemyShipThisFrame)
            {
                insideEnemyShip = false;
            }
        }
        else
        {
            insideEnemyShip = false;
        }
    }

    bool alreadyExploded = false;

    private void Explode()
    {
        if (!alreadyExploded)
        {
            SpawnEffect(EffectType.explode, lastHitPoint, transform.rotation);
            alreadyExploded = true;
            Destroy(gameObject);
        }
    }

    void SetDirAtMovingDir()
    {
        transform.eulerAngles = new Vector3(0, 0, DataOperator.GetVector2DirInDegrees(myRigidbody2D.velocity));
    }

    void SpawnEffect(EffectType effectType, Vector3 position, Quaternion rotation)
    {
        switch (effectType)
        {
            case EffectType.shipPenetration:
                RpcHandlerForEffects.SpawnEffectOnClients(new NetworkString(shipPenetrationEffects), position, rotation);
                break;
            case EffectType.moduleHit:
                RpcHandlerForEffects.SpawnEffectOnClients(new NetworkString(moduleHitEffects), position, rotation);
                break;
            case EffectType.explode:
                RpcHandlerForEffects.SpawnEffectOnClients(new NetworkString(explodeEffects), position, rotation);
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
            Gizmos.DrawIcon(myPosition + raycastPosition, "Raycast icon.png", false);
        }
    }
#endif
}
