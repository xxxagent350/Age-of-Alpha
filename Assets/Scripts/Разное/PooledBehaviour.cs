using UnityEngine;

public abstract class PooledBehaviour : MonoBehaviour
{
    public virtual void OnSpawnedFromPool()
    {
        //для переопределения наследуемыми классами, вызывается при создании или повторном использовании объекта из пула
        //здесь рекомендуется восстановить значения переменных и другие параметры в исходное состояние
    }

    public virtual void OnReturnedToPool()
    {
        //для переопределения наследуемыми классами, вызывается при возвращении объекта в пул
    }
}
