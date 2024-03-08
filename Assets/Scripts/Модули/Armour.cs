using UnityEngine;

public class Armour : MonoBehaviour
{
    [Header("Настройка")]
    public float maxHP = 100;

    [Header("Отладка")]
    [SerializeField] float HP;

    private void Start()
    {
        HP = maxHP;
    }

    public void Damage(float damage)
    {
        HP -= damage;
        if (HP <= 0)
        {
            HP = 0;
            Destroy(gameObject);
        }
    }
}
