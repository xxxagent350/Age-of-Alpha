using UnityEngine;

public class EffectSmoothDisabler : MonoBehaviour
{
    [Header("Задержка перед включением")]
    [SerializeField] Limits enablingDelay;
    [Header("Время жизни эффекта")]
    [SerializeField] Limits timelife;
    [Header("Задержка перед удалением")]
    [SerializeField] float deleteDelay = 1;

    ParticleSystem particleSystem_;
    ParticleSystem[] particleSystemsInChildren;

    void Start()
    {
        particleSystem_ = GetComponent<ParticleSystem>();
        particleSystemsInChildren = GetComponentsInChildren<ParticleSystem>();

        particleSystem_.Stop(true);
        Invoke(nameof(EnableParticles), enablingDelay.GetRandomValue());
        
    }

    void EnableParticles()
    {
        particleSystem_.Play(true);
        Invoke(nameof(DisableEmission), timelife.GetRandomValue());
    }

    void DisableEmission()
    {
        for (int arrayNum = 0; arrayNum < particleSystemsInChildren.Length; arrayNum++)
        {
            ParticleSystem.EmissionModule particleSystemLocalVar = particleSystemsInChildren[arrayNum].emission;
            particleSystemLocalVar.rateOverTime = 0;
        }
        Destroy(gameObject, deleteDelay);
    }
}
