using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RocketEngine : NetworkBehaviour
{
    [Header("���������")]
    [Tooltip("���������� ���� ��������� Projectile, ������� ������ ������ �� ���� �� �������")]
    [SerializeField] private Projectile _myProjectile;
    [Tooltip("�������� ����� ���������� ���������� �������������� ������")]
    [SerializeField] private float _delayBeforeEnablingEngines = 0.5f;
    [Tooltip("���������")]
    [SerializeField] private float _acceleration = 100f;
    [Tooltip("������������ ��������")]
    public float MaxSpeed = 180f;
    [Tooltip("������. �� ��� ������ ����������� ���� ������������ ��������")]
    [SerializeField] private float _frictionMod = 0.1f;
    [Tooltip("'�������' ������. ���������� ����� �������� �� ���� ������ ��������� ������ �����")]
    [SerializeField] private float _sideFrictionMod = 0.1f;
    [Tooltip("������������ �������� ��������. 360 = ������ ������ �� 1 �������")]
    public float MaxRotateSpeed = 150f;
    [Tooltip("������������ ��������� ������ �����. ���� ����� �� ����������, ����� ������ �����")]
    public float TargetsSearchingRadius = 200f;
    [Tooltip("���� ������ ��������� �� RpcHandlerForEffects")]
    [SerializeField] private string _engineStartSound;
    [Tooltip("�������� ����� ������ ���������")]
    [SerializeField] private AudioSource _engineAudioSource;
    [Tooltip("�������, ������� ����� ������������ ����� ������ ���������")]
    [SerializeField] private List<ParticleSystem> _flightEffects;
    [Tooltip("�������� �������� ��������� ������")]
    [SerializeField] private GameObject _flightEffectsParent;
    [Tooltip("�������� ����� ��������� �������� �����, ��������� ����� ������� ����� ������� ����� ������ �����������")]
    [SerializeField] private float _flightEffectsDestroyDelay = 10;

    [Header("�������")]
    [SerializeField] private Transform _target;
    [SerializeField] private bool _enginesEnabled = false;

    private Rigidbody2D _myRigidbody2D;


    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            GetComponents();
            StartCoroutine(WaitingToEnableEnginesCoroutine());
        }
    }

    private void GetComponents()
    {
        _myRigidbody2D = GetComponent<Rigidbody2D>();
    }

    private IEnumerator WaitingToEnableEnginesCoroutine()
    {
        yield return new WaitForSeconds(_delayBeforeEnablingEngines);
        _enginesEnabled = true;
        ChangeStateFlightEffectsRpc(true);
    }

    [Rpc(SendTo.Everyone)]
    private void ChangeStateFlightEffectsRpc(bool newState)
    {
        ChangeStateFlightEffects(newState);
    }

    private void ChangeStateFlightEffects(bool newState)
    {
        foreach (var flightEffect in _flightEffects)
        {
            if (newState == true)
            {
                flightEffect.Play();
            }
            else
            {
                flightEffect.Stop();
            }
        }
        if (newState == true)
        {
            _engineAudioSource.Play();
            RpcHandlerForEffects.SpawnEffectLocal(_engineStartSound, transform.position, Quaternion.identity, Vector3.zero);
        }
        else
        {
            _engineAudioSource.Stop();
        }
    }

    private void FixedUpdate()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (_enginesEnabled)
            {
                SearchForTargets();
                Accelerate();
                Friction();
                if (_target != null)
                {
                    Rotate();
                }
            }
        }
    }

    private void SearchForTargets()
    {
        if (_target != null && Vector2.Distance(transform.position, _target.transform.position) > TargetsSearchingRadius)
        {
            _target = null;
        }

        if (!_target)
        {
            //���� ���������� �����
            List<GameObject> shipsInRadius = new();
            foreach (GameObject ship in GameObjectsSearcher.GetAllShipGameObjects())
            {
                if (Vector2.Distance(transform.position, ship.transform.position) <= TargetsSearchingRadius)
                {
                    shipsInRadius.Add(ship);
                }
            }

            for (int shipNum = 0; shipNum < shipsInRadius.Count; shipNum++)
            {
                ShipGameStats shipGameStats = shipsInRadius[shipNum].GetComponent<ShipGameStats>();
                if (shipGameStats == null || _myProjectile.TeamID.Value.String == shipGameStats.TeamID.Value.String || shipGameStats.Destroyed.Value == true)
                {
                    shipsInRadius.Remove(shipsInRadius[shipNum]);
                    shipNum--;
                }
            }

            float distanceToNearestSEnemyShip = TargetsSearchingRadius + 1;
            GameObject nearestEnemyShip = null;

            foreach (GameObject enemyShip in shipsInRadius)
            {
                float distanceToEnemyShip = Vector2.Distance(transform.position, enemyShip.transform.position);
                if (distanceToEnemyShip < distanceToNearestSEnemyShip)
                {
                    distanceToNearestSEnemyShip = distanceToEnemyShip;
                    nearestEnemyShip = enemyShip;
                }
            }

            if (nearestEnemyShip != null)
            {
                _target = nearestEnemyShip.transform;
            }
        }
    }

    private void Accelerate()
    {
        Vector2 force = new(0, _acceleration * _myRigidbody2D.mass);
        _myRigidbody2D.AddRelativeForce(force);
    }

    private void Friction()
    {
        //�������� ������
        float currentSpeed = _myRigidbody2D.linearVelocity.magnitude;
        if (currentSpeed > MaxSpeed)
        {
            _myRigidbody2D.linearDamping = (currentSpeed - MaxSpeed) * _frictionMod;
        }
        else
        {
            _myRigidbody2D.linearDamping = 0;
        }

        //"�������" ������. �� ��� ������ ��������� ������� ������ �����
        float localXSpeed = DataOperator.RotateVector2(_myRigidbody2D.linearVelocity, -transform.eulerAngles.z).x;
        Vector2 frictionLocalForce = new Vector2(-localXSpeed * _myRigidbody2D.mass * _sideFrictionMod, 0);
        _myRigidbody2D.AddRelativeForce(frictionLocalForce);

    }

    private void Rotate()
    {
        float currentAngle = transform.eulerAngles.z;
        Vector2 deltaPosition = _target.position - transform.position;
        float targetAngle = Vector2.SignedAngle(Vector2.up, deltaPosition);
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, MaxRotateSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(0, 0, newAngle);
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            _myProjectile.OnProjectileDestroy += DisableFlightEffectsRpc;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            _myProjectile.OnProjectileDestroy -= DisableFlightEffectsRpc;
        }
    }

    [Rpc(SendTo.Everyone)]
    private void DisableFlightEffectsRpc()
    {
        _flightEffectsParent.transform.parent = null;
        ChangeStateFlightEffects(false);
        Destroy(_flightEffectsParent, _flightEffectsDestroyDelay);
    }
}
