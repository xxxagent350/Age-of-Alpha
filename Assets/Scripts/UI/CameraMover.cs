using UnityEngine;

public class CameraMover : MonoBehaviour
{
    //[Header("���������")]
    

    [Header("�������")]
    [SerializeField] Transform playerShipTransform;

    public static CameraMover instance;
    bool playerNotSpawned = true;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("�� ����� ��������� CameraMover, ���� ���� �� ������");
        }
        else
        {
            instance = this;
        }
    }

    public void SetPlayerShip(Transform playerShipTransform_)
    {
        playerShipTransform = playerShipTransform_;
        LateUpdate();
    }

    private void LateUpdate()
    {
        if (playerShipTransform == null)
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
        transform.position = new Vector3(playerShipTransform.position.x, playerShipTransform.position.y, transform.position.z);
    }

    void TeleportToPlayer()
    {
        transform.position = new Vector3(playerShipTransform.position.x, playerShipTransform.position.y, transform.position.z);
    }
}
