using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] float accelerationMod;
    [SerializeField] float accelerationOverflightMod;
    [SerializeField] float maxAcceleration;
    [SerializeField] float maxDeceleration;
    [SerializeField] float maxSpeed;
    [SerializeField] float ignoredDistance = 0.1f;
    [SerializeField] float ignoredSpeed = 0.3f;

    [Header("Отладка")]
    public Transform playerTransform;
    [SerializeField] Vector2 speed;
    [SerializeField] Vector2 acceleration;
    [SerializeField] Vector2 distance;
    [SerializeField] Vector2 fullDistance;
    [SerializeField] Vector2 physicalDistance;

    bool playerNotSpawned = true;

    private void LateUpdate()
    {
        if (playerTransform == null)
        {
            playerNotSpawned = true;
        }
        else
        {
            if (playerNotSpawned)
            {
                playerNotSpawned = false;
                TeleportToPlayer();
            }
            MoveCamera();
        }
    }

    public void MoveCamera()
    {
        transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, transform.position.z);
    }

    void TeleportToPlayer()
    {
        transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, transform.position.z);
    }
}
