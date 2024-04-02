using UnityEngine;

public class CameraMover : MonoBehaviour
{
    //[Header("Настройка")]
    

    [Header("Отладка")]
    public Transform playerTransform;

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
