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

    [Header("Отладка")]
    [Tooltip("Команда корабля, который выпустил снаряд")]
    public string teamID = "none";

    Rigidbody2D myRigidbody2D;
    LayerMask damagableLayers;

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
            
            damagableLayers = LayerMask.GetMask("Module", "Projectile", "Environment");
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

        RaycastHit2D[] mainLayerHits = Physics2D.RaycastAll(position, direction, distanceToCheck, damagableLayers);

        if (mainLayerHits.Length > 0)
        {
            foreach (RaycastHit2D hitInfo in mainLayerHits)
            {
                if (hitInfo.collider != null)
                {
                    LayerMask hitInfoLayer = hitInfo.collider.gameObject.layer;

                    if (hitInfoLayer == LayerMask.NameToLayer("Projectile"))
                    {
                        Projectile hittedProjectile = hitInfo.collider.GetComponent<Projectile>();
                        if (hittedProjectile != null && hittedProjectile.teamID != teamID)
                        {
                            hittedProjectile.damage.DamageOtherDamage(damage);
                        }
                    }
                    if (hitInfoLayer == LayerMask.NameToLayer("Environment") || hitInfoLayer == LayerMask.NameToLayer("Module"))
                    {
                        Durability hittedObject = hitInfo.collider.GetComponent<Durability>();
                        if (hittedObject != null && hittedObject.teamID != teamID)
                        {
                            hittedObject.durability.TakeDamage(damage);
                        }
                    }
                    if (selfDestructAfterHit)
                    {
                        Explode();
                    }
                }
            }
        }
    }

    bool alreadyExploded = false;
    private void Explode()
    {
        if (!alreadyExploded)
        {
            alreadyExploded = true;
            Destroy(gameObject);
        }
    }

    void SetDirAtMovingDir()
    {
        transform.eulerAngles = new Vector3(0, 0, DataOperator.GetVector2DirInDegrees(myRigidbody2D.velocity));
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
