using UnityEngine;

public class PlayUISound : MonoBehaviour
{
    [SerializeField] AudioClip sound;
    [SerializeField] float volume = 1;

    public void Play()
    {
        if (sound != null)
            DataOperator.instance.PlayUISound(sound, volume);
    }
}
