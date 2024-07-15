using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//класс отвечает за таран корабля
public class ShipRammer : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] private RamEffectsData _ramEffectsData;
    public bool DamageAllies = false;

    private Rigidbody2D _rigidbody2D;
    private ShipGameStats _myShipGameStats;
    private Vector2 _lastFrameVelocity;

    private const float RamDamageMod = 0.0015f;
    private const float MinDamageForLowDamageEffects = 0.5f;
    private const float MinDamageForMediumDamageEffects = 3f;
    private const float MinDamageForHighDamageEffects = 10f;

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _myShipGameStats = GetComponent<ShipGameStats>();
        }
        else
        {
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            _lastFrameVelocity = _rigidbody2D.velocity;
        } 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (DamageAllies)
            {
                SpawnShockWave(collision);
            }
            else
            {
                ShipGameStats shipGameStats = collision.collider.GetComponent<ShipGameStats>();
                if (shipGameStats != null)
                {
                    if (shipGameStats.TeamID != _myShipGameStats.TeamID)
                    {
                        SpawnShockWave(collision);
                    }
                }
                else
                {
                    SpawnShockWave(collision);
                }
            }
        }
    }

    private void SpawnShockWave(Collision2D collision)
    {
        float deltaVelocity = (_rigidbody2D.velocity - _lastFrameVelocity).magnitude;
        float shockWavePower = deltaVelocity * _rigidbody2D.mass * RamDamageMod;

        shockWavePower /= collision.contacts.Length;
        foreach (ContactPoint2D contactPoint in collision.contacts)
        {
            ShockWave.CreateShockWave(shockWavePower, contactPoint.point);
            SpawnRamEffects(shockWavePower, contactPoint.point);
        }
    }

    private void SpawnRamEffects(float shockWavePower, Vector2 position)
    {
        List<string> effectsToSpawn = new List<string>(0);
        if (shockWavePower < MinDamageForLowDamageEffects)
        {
            effectsToSpawn = _ramEffectsData.TouchEffects;
        }
        if (shockWavePower >= MinDamageForLowDamageEffects && shockWavePower <= MinDamageForMediumDamageEffects)
        {
            effectsToSpawn = _ramEffectsData.LowDamageEffects;
        }
        if (shockWavePower > MinDamageForMediumDamageEffects && shockWavePower <= MinDamageForHighDamageEffects)
        {
            effectsToSpawn = _ramEffectsData.MediumDamageEffects;
        }
        if (shockWavePower > MinDamageForHighDamageEffects)
        {
            effectsToSpawn = _ramEffectsData.HighDamageEffects;
        }
        RpcHandlerForEffects.SpawnEffectsOnClients(effectsToSpawn, position, Quaternion.identity, _rigidbody2D.velocity);
    }
}
