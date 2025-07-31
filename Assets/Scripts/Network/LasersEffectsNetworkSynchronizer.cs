using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class LasersEffectsNetworkSynchronizer : NetworkBehaviour
{
    private Dictionary<LowAccuracyVector2, SpriteRenderer> _lasersOnShip = new(0);
    private Dictionary<LowAccuracyVector2, AudioSource> _lasersAudioSourcesOnShip = new(0);

    [Rpc(SendTo.Everyone)]
    public void UpdateLaserEffectsRpc(LowAccuracyVector2 localPosition, float localRotationDegrees, NetworkString laserEffectName, float laserDistance, float laserPowerMod, NetworkString soundEffectName, float soundVolumeMod)
    {
        //картинка
        SpriteRenderer laserSpriteRenderer;
        if (_lasersOnShip.TryGetValue(localPosition, out laserSpriteRenderer) == false)
        {
            GameObject newLaserGameObject = RpcHandlerForEffects.instance.SpawnEffect(laserEffectName.String, Vector3.zero, Quaternion.identity, Vector3.zero)[0];
            laserSpriteRenderer = newLaserGameObject.GetComponentInChildren<SpriteRenderer>();
            newLaserGameObject.transform.parent = transform;
            newLaserGameObject.transform.localPosition = localPosition.GetVector2();
            _lasersOnShip.Add(localPosition, laserSpriteRenderer);
        }
        laserSpriteRenderer.transform.parent.localEulerAngles = new Vector3(0, 0, localRotationDegrees);
        Vector3 oldLocalScale = laserSpriteRenderer.transform.parent.localScale;
        laserSpriteRenderer.transform.parent.localScale = new Vector3(oldLocalScale.x, laserDistance, oldLocalScale.z);
        Color oldColor = laserSpriteRenderer.color;
        laserSpriteRenderer.color = new Color(oldColor.r, oldColor.b, oldColor.g, laserPowerMod);

        //звук
        AudioSource laserAudioSource;
        if (_lasersAudioSourcesOnShip.TryGetValue(localPosition, out laserAudioSource) == false)
        {
            GameObject newSoundGameObject = RpcHandlerForEffects.instance.SpawnEffect(soundEffectName.String, Vector3.zero, Quaternion.identity, Vector3.zero)[0];
            laserAudioSource = newSoundGameObject.GetComponent<AudioSource>();
            laserAudioSource.transform.parent = transform;
            laserAudioSource.transform.localPosition = localPosition.GetVector2();
            _lasersAudioSourcesOnShip.Add(localPosition, laserAudioSource);
        }
        laserAudioSource.volume = laserPowerMod * soundVolumeMod;
    }
}
