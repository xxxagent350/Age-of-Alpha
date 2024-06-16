using System.Collections.Generic;
using UnityEngine;

public class PooledSound : PooledBehaviour
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
