using UnityEngine;

public class PooledEffect : PooledBehaviour
{
    [Header("���������")]
    [SerializeField] float disablingDelay;

    [Header("�������")]
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
