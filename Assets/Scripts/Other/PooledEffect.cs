using UnityEngine;

public class PooledEffect : PooledBehaviour
{
    [Header("���������")]
    [SerializeField] private float disablingDelay;

    private ParticleSystem[] myParticleSystems;
    [HideInInspector] public Vector3 speed = Vector3.zero; 

    void Awake()
    {
        myParticleSystems = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem particleSystem in myParticleSystems)
        {
            var particleSystemMain = particleSystem.main;
            particleSystemMain.stopAction = ParticleSystemStopAction.None;
        }
    }

    public override void OnSpawnedFromPool()
    {
        Invoke(nameof(Disable), disablingDelay);

        foreach (ParticleSystem particleSystem in myParticleSystems)
        {
            particleSystem.Play();
        }
    }

    public virtual void Update()
    {
        transform.position += speed * Time.deltaTime;
    }

    void Disable()
    {
        PoolingSystem.Instance.ReturnGOToPool(gameObject);
    }
}
