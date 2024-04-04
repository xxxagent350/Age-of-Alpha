using UnityEngine;
using Unity.Netcode;

public class Bullet : MonoBehaviour
{
    [Header("Настройка")]
    [Tooltip("Урон")]
    public float damage;
    [Tooltip("Стартовая скорость")]
    public float speed;
    [Tooltip("Максимальное время существования снаряда")]
    public float lifetime;
    [Tooltip("Тип урона")]
    public DamageTypes damageType;

    [Header("Отладка")]
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
