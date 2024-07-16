using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class LasersEffectsNetworkSynchronizer : NetworkBehaviour
{
    private Dictionary<LowAccuracyVector2, SpriteRenderer> _lasersOnShip = new(0);

    public override void OnDestroy()
    {
        foreach (SpriteRenderer spriteRenderer in _lasersOnShip.Values)
        {
            PoolingSystem.Instance.ReturnGOToPool(spriteRenderer.transform.parent.gameObject);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void VisualizeLaserRpc(LowAccuracyVector2 localPosition, float localRotationDegrees, NetworkString laserEffectName, float laserDistance, float laserAlpha)
    {
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
        laserSpriteRenderer.color = new Color(oldColor.r, oldColor.b, oldColor.g, laserAlpha);
    }
}
