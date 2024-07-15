using UnityEngine;
using System.Collections;

public class MusicController : MonoBehaviour
{
    [Header("���������")]
    [SerializeField] private AudioClip _calmMusic;
    [SerializeField] private float _calmMusicVolumeMod = 1;
    [SerializeField] private AudioClip _aggressiveMusic;
    [SerializeField] private float _aggersiveMusicVolumeMod = 1;
    [SerializeField] private AudioSource _calmChannelAudioSource;
    [SerializeField] private AudioSource _aggressiveChannelAudioSource;

    [Header("�������")]
    public MusicType TargetMusicType = MusicType.calmMusic;
    [SerializeField] private MusicType _musicPlayingType = MusicType.none;

    private const float MusicFadeTime = 3; //����� ��������� / ��������� ������

    private float _maxMusicLength;

    private void Start()
    {
        if (_calmMusic != null && _aggressiveMusic != null)
        {
            _calmChannelAudioSource.clip = _calmMusic;
            _aggressiveChannelAudioSource.clip = _aggressiveMusic;
            _maxMusicLength = Mathf.Max(_calmMusic.length, _aggressiveMusic.length) + 0.1f;
            StartCoroutine(PlayCalmAndAggresiveMusic());
        }
    }

    private IEnumerator PlayCalmAndAggresiveMusic()
    {
        while (true)
        {
            _calmChannelAudioSource.Play();
            _aggressiveChannelAudioSource.Play();
            yield return new WaitForSecondsRealtime(_maxMusicLength);
        }
    }

    private void FixedUpdate()
    {
        if (TargetMusicType == MusicType.none)
        {
            //�������� ����� �� � ����� � ������ � �����������
            FadeMusicChannel(false, MusicChannel.all);
        }
        if (TargetMusicType == MusicType.calmMusic)
        {
            //���� ������ ��������� ������
            if (_musicPlayingType != MusicType.calmMusic)
            {
                //������ ������ ������, ���������
                if (_aggressiveChannelAudioSource.volume > 0)
                {
                    FadeMusicChannel(false, MusicChannel.aggressive);
                }
                else
                {
                    //�������� ��������� ����� ��� ����������
                    _musicPlayingType = MusicType.calmMusic;
                }
            }
            else
            {
                FadeMusicChannel(true, MusicChannel.calm);
            }
        }
        if (TargetMusicType == MusicType.aggressiveMusic)
        {
            //���� ������ ����������� ������
            if (_musicPlayingType != MusicType.aggressiveMusic)
            {
                //������� ����������� ���� ��� ����� ��� �����
                _musicPlayingType = MusicType.aggressiveMusic;
            }
            else
            {
                FadeMusicChannel(true, MusicChannel.aggressive);
                FadeMusicChannel(true, MusicChannel.calm);
            }
        }
    }

    void FadeMusicChannel(bool turnOn, MusicChannel musicChannel)
    {
        if (musicChannel == MusicChannel.calm || musicChannel == MusicChannel.all)
        {
            FadeAudioSource(turnOn, musicChannel, _calmChannelAudioSource);
        }
        if (musicChannel == MusicChannel.aggressive || musicChannel == MusicChannel.all)
        {
            FadeAudioSource(turnOn, musicChannel, _aggressiveChannelAudioSource);
        }
    }

    void FadeAudioSource(bool turnOn, MusicChannel musicChannel, AudioSource audioSource)
    {
        float channelMusicMod = 1;
        switch (musicChannel)
        {
            case MusicChannel.calm:
                channelMusicMod = _calmMusicVolumeMod;
                break;
            case MusicChannel.aggressive:
                channelMusicMod = _aggersiveMusicVolumeMod;
                break;
        }
        
        if (turnOn)
        {
            if (audioSource.volume < channelMusicMod)
            {
                audioSource.volume += (Time.deltaTime / MusicFadeTime) * channelMusicMod;
            }
            else
            {
                audioSource.volume = channelMusicMod;
            }
        }
        else
        {
            if (audioSource.volume > 0)
            {
                audioSource.volume -= (Time.deltaTime / MusicFadeTime) * channelMusicMod;
            }
            else
            {
                audioSource.volume = 0;
            }
        }
    }

    public enum MusicType
    {
        none,
        calmMusic,
        aggressiveMusic,
    }

    public enum MusicChannel
    {
        calm,
        aggressive,
        all
    }
}
