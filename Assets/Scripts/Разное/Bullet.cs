using UnityEngine;
using Unity.Netcode;

public class Bullet : MonoBehaviour
{
    [Header("���������")]
    [Tooltip("����")]
    public float damage;
    [Tooltip("��������� ��������")]
    public float speed;
    [Tooltip("������������ ����� ������������� �������")]
    public float lifetime;
    [Tooltip("��� �����")]
    public DamageTypes damageType;

    [Header("�������")]
    public string myTeamID;

    Rigidbody2D myRigidbody2D;

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Destroy(gameObject, lifetime);
            myRigidbody2D = GetComponent<Rigidbody2D>();
            myRigidbody2D.velocity += DataOperator.RotateVector2(new Vector2(0, speed), transform.eulerAngles.z);
        }
    }
}
