using UnityEngine;

public class PooledEffect : PooledBehaviour
{
    [Header("Настройка")]
    [SerializeField] float disablingDelay;

    [Header("Отладка")]
    [SerializeField] ParticleSystem[] myParticleSystems;

    void Awake()
    {
        myParticleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    public override void OnSpawnedFromPool()
    {
        Invoke(nameof(Disable), disablingDelay);

        foreach (ParticleSystem particleSystem in myParticleSystems)
        {
            particleSystem.Play();
        }
    }

    void Disable()
    {
        PoolingSystem.instance.ReturnGOToPool(gameObject);
    }
}
