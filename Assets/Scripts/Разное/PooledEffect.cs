using UnityEngine;

public class PooledEffect : PooledBehaviour
{
    ParticleSystem[] myParticleSystems;

    void Start()
    {
        myParticleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    public override void OnSpawnedFromPool()
    {
        
    }
}
