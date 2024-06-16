using System.Collections.Generic;
using UnityEngine;

public class PooledSound : PooledBehaviour
{
    [Header("Настройка")]
    [Tooltip("Пределы модификации громкости для разнообразия звуков")]
    [SerializeField] FloatInterval volumeRandomizedMod = new FloatInterval(1, 1);
    [Tooltip("Пределы модификации pitch для разнообразия звуков")]
    [SerializeField] FloatInterval pitchRandomizedMod = new FloatInterval(1, 1);

    [Tooltip("Если это поле заполнено, буде проигран случайный из аудиоклипов в списке")]
    [SerializeField] List<AudioClip> audioClips;

    //стартовые значения источника звука
    float startVolume;
    float startPitch;

    AudioSource audioSource;
    float clipLength;

    public override void Initialize()
    {
        audioSource = GetComponent<AudioSource>();

        startVolume = audioSource.volume;
        startPitch = audioSource.pitch;
    }

    public override void OnSpawnedFromPool()
    {
        audioSource.volume = startVolume * volumeRandomizedMod.GetRandomValueFromInterval();
        audioSource.pitch = startPitch * pitchRandomizedMod.GetRandomValueFromInterval();
        if (audioClips.Count > 0)
        {
            audioSource.clip = audioClips[Random.Range(0, audioClips.Count)];
        }

        clipLength = audioSource.clip.length + 0.1f;
        audioSource.Play();
        Invoke(nameof(ReturnToPool), clipLength);
    }

    void ReturnToPool()
    {
        PoolingSystem.instance.ReturnGOToPool(gameObject);
    }
}
