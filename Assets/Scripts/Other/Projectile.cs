using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [Header("���������")]
    [Tooltip("����")]
    public Damage Damage;
    [Tooltip("���� �������� ����� ��� ������ �������")]
    public float ShockWavePower;
    [Tooltip("������� ������ ���� ��� ������ �����, ����������� ���� ���� ���������� �������")]
    [SerializeField] private Vector2 _raycastPosition;
    [Tooltip("��������� ��������")]
    public float StartSpeed;
    [Tooltip("�����. �� �� ������� ������� ��� �������� � ���������")]
    public float Mass;
    [Tooltip("������������ ����� ������������� �������")]
    public float Lifetime;
    [Tooltip("����������� �� ������ ����������� ����� ������� ���������? ���� ���������, ������ ����� ������, ������� �������� ������ � ��������� �������, ���� � ���� �� �������� ����")]
    [SerializeField] private bool _selfDestructAfterHit;
    [Tooltip("���������� �� ����� ������������� ����� ����� �������? ���� ���������, ������ ������ �������")]
    [SerializeField] private bool _explodeOnLifetimeEnds = false;
    [Tooltip("���� ��������, ����� ������������ �������� � ������� ��������")]
    [SerializeField] private bool _rotateToVelocityVectorDir = false;

    [Tooltip("������� �������� ������� ������� (������� �������� �������� �� ������� RpcHandlerForEffects)")]
    [SerializeField] private List<string> _shipPenetrationEffects;
    [Tooltip("������� ��������� � ������ �������, �������, ��������� � ������ ������� ������� (������� �������� �������� �� ������� RpcHandlerForEffects)")]
    [SerializeField] private List<string> _moduleHitEffects;
    [Tooltip("������� ������ ������� (������� �������� �������� �� ������� RpcHandlerForEffects)")]
    [SerializeField] private List<string> _explodeEffects;

    [Header("�������")]
    [Tooltip("������� �������, ������� �������� ������")]
    public NetworkVariable<NetworkString> TeamID = new();

    private Rigidbody2D _myRigidbody2D;
    private bool _insideEnemyShip;
    private float _startFullDamage;

    [Tooltip("��������� �����, � ������� ��� ������ ���� (����� ��� ����������� ��������� ������� ������)")]
    private Vector3? _lastHitPoint = null;
    [Tooltip("�������� ���������� �������, � ������� �������� ������ (����� ��� �������� �������� ��� ������ �������)")]
    private Vector3 _lastHitCollidersSpeed = Vector3.one;

    public delegate void OnProjectileDestroyContainer();
    public event OnProjectileDestroyContainer OnProjectileDestroy;

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
            _myRigidbody2D.linearVelocity += DataOperator.RotateVector2(Vector2.up * StartSpeed, transform.eulerAngles.z);
        }
    }

    public void ApplyImpulseToParentShip(Rigidbody2D parentShipRigidbody)
    {
        //������� ����������� �������
        Vector2 force = DataOperator.RotateVector2(Vector2.down * StartSpeed * Mass, transform.eulerAngles.z);
        Vector2 position = transform.position;
        parentShipRigidbody.AddForceAtPosition(force, position);
    }

    public void ApplyImpulseToTargetShip(Rigidbody2D targetShipRigidbody, float damagePartUsed)
    {
        //������� ������� � �������� ������
        if (targetShipRigidbody != null)
        {
            Vector2 force = (_myRigidbody2D.linearVelocity - targetShipRigidbody.linearVelocity) * damagePartUsed * Mass;
            Vector2 position = transform.position;
            if (force.magnitude > 0.01f)
            {
                targetShipRigidbody.AddForceAtPosition(force, position);
            }
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
        float distanceToCheck = _myRigidbody2D.linearVelocity.magnitude * Time.fixedDeltaTime;

        Vector3 position = transform.position + (Vector3)DataOperator.RotateVector2(_raycastPosition, transform.eulerAngles.z);
        Vector3 direction = _myRigidbody2D.linearVelocity.normalized;

        RaycastHit2D[] mainLayerHits = Physics2D.RaycastAll(position, direction, distanceToCheck);

        if (mainLayerHits.Length > 0)
        {
            bool insideEnemyShipThisFrame = false;
            foreach (RaycastHit2D hitInfo in mainLayerHits)
            {
                if (hitInfo.collider != null)
                {
                    LayerMask hitInfoLayer = hitInfo.collider.gameObject.layer;

                    Rigidbody2D collidersRigidbody2D = hitInfo.collider.GetComponentInParent<Rigidbody2D>();
                    Vector3 collidersSpeed = Vector3.zero;
                    if (collidersRigidbody2D != null)
                    {
                        collidersSpeed = collidersRigidbody2D.linearVelocity;
                        _lastHitCollidersSpeed = collidersSpeed;
                    }

                    if (hitInfoLayer == LayerMask.NameToLayer("Ship"))
                    {
                        ShipGameStats shipGameStats = hitInfo.collider.GetComponent<ShipGameStats>();
                        if (shipGameStats != null && shipGameStats.TeamID.Value.String != TeamID.Value.String)
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
                            if (hittedProjectile != null && hittedProjectile.TeamID.Value.String != TeamID.Value.String)
                            {
                                SpawnEffect(EffectType.moduleHit, hitInfo.point, transform.rotation, collidersSpeed);
                                hittedProjectile.Damage.DamageOtherDamage(Damage);
                                _lastHitPoint = hitInfo.point;
                            }
                        }
                        if (hitInfoLayer == LayerMask.NameToLayer("Environment") || hitInfoLayer == LayerMask.NameToLayer("Module"))
                        {
                            Durability hittedObject = hitInfo.collider.GetComponent<Durability>();
                            if (hittedObject != null && hittedObject.TeamID != TeamID.Value.String)
                            {
                                SpawnEffect(EffectType.moduleHit, hitInfo.point, transform.rotation, collidersSpeed);
                                float previousDamage = Damage.GetAllDamage();
                                hittedObject.durability.TakeDamage(Damage);
                                float deltaDamage = previousDamage - Damage.GetAllDamage();
                                ApplyImpulseToTargetShip(hitInfo.collider.GetComponentInParent<Rigidbody2D>(), deltaDamage / _startFullDamage);
                                _lastHitPoint = hitInfo.point;
                                if (_selfDestructAfterHit)
                                {
                                    Explode();
                                }
                            }
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
            OnProjectileDestroy?.Invoke();

            if (_lastHitPoint == null)
            {
                _lastHitPoint = transform.position;
            }

            ShockWave.CreateShockWave(ShockWavePower, _lastHitPoint.Value);
            if (_lastHitCollidersSpeed == Vector3.one)
            {
                SpawnEffect(EffectType.explode, _lastHitPoint.Value, transform.rotation, _myRigidbody2D.linearVelocity);
            }
            else
            {
                SpawnEffect(EffectType.explode, _lastHitPoint.Value, transform.rotation, _lastHitCollidersSpeed);
            }
            alreadyExploded = true;
            Destroy(gameObject);
        }
    }

    void SetDirAtMovingDir()
    {
        transform.eulerAngles = new Vector3(0, 0, DataOperator.GetVector2DirInDegrees(_myRigidbody2D.linearVelocity));
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
    //��������� ������� ������ raycast
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
