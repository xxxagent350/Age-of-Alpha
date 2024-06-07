using UnityEngine;
using Unity.Netcode;

public class Projectile : MonoBehaviour
{
    [Header("���������")]
    [Tooltip("����")]
    public Damage damage;
    [Tooltip("������� ������ ���� ��� ������ �����, ����������� ���� ���� ���������� �������")]
    [SerializeField] Vector2 raycastPosition;
    [Tooltip("��������� ��������")]
    public float startSpeed;
    [Tooltip("������������ ����� ������������� �������")]
    public float lifetime;
    [Tooltip("��� �����")]
    [SerializeField] DamageTypes damageType;
    [Tooltip("����������� �� ������ ����������� ����� ������� ���������? ���� ���������, ������ ����� ������, ������� �������� ������ � ��������� �������, ���� � ���� �� �������� ����")]
    [SerializeField] bool selfDestructAfterHit;
    [Tooltip("���������� �� ����� ������������� ����� ����� �������? ���� ���������, ������ ������ �������")]
    [SerializeField] bool explodeOnLifetimeEnds = false;
    [Tooltip("���� ��������, ����� ������������ �������� � ������� ��������")]
    [SerializeField] bool rotateToVelocityVectorDir = false;

    [Tooltip("������ �������� ������� ������� (������� �������� ������� �� ������� RpcHandlerForEffects)")]
    [SerializeField] string shipPenetrationEffects;
    [Tooltip("������ ��������� � ������ �������, �������, ��������� � ������ ������� ������� (������� �������� ������� �� ������� RpcHandlerForEffects)")]
    [SerializeField] string moduleHitEffects;
    [Tooltip("������ ������ ������� (������� �������� ������� �� ������� RpcHandlerForEffects)")]
    [SerializeField] string explodeEffects;

    [Header("�������")]
    [Tooltip("������� �������, ������� �������� ������")]
    public string teamID = "none";

    Rigidbody2D myRigidbody2D;
    bool insideEnemyShip;

    [Tooltip("��������� �����, � ������� ��� ������ ���� (����� ��� ����������� ��������� ������� ������)")]
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
    //��������� ������� ������ raycast
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
