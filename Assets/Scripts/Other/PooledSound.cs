using System.Collections.Generic;
using UnityEngine;

public class PooledSound : PooledEffect
{
    [Header("���������")]
    [Tooltip("������� ����������� ��������� ��� ������������ ������")]
    [SerializeField] FloatInterval volumeRandomizedMod = new FloatInterval(1, 1);
    [Tooltip("������� ����������� pitch ��� ������������ ������")]
    [SerializeField] FloatInterval pitchRandomizedMod = new FloatInterval(1, 1);

    [Tooltip("���� ��� ���� ���������, ���� �������� ��������� �� ����������� � ������")]
    [SerializeField] List<AudioClip> audioClips;

    //��������� �������� ��������� �����
    float startVolume;
    float startPitch;

    AudioSource audioSource;
    float clipLength;

    public const float MinDistance = 100;
    public const float MaxDistance = 10000;

    public override void Initialize()
    {
        audioSource = GetComponent<AudioSource>();
        startVolume = audioSource.volume;
        startPitch = audioSource.pitch;

        audioSource.minDistance = MinDistance;
        audioSource.maxDistance = MaxDistance;
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
        PoolingSystem.Instance.ReturnGOToPool(gameObject);
    }
}
