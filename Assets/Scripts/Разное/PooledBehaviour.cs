using UnityEngine;

public abstract class PooledBehaviour : MonoBehaviour
{
    public virtual void Initialize()
    {
        //вызывается при создании объекта при помощи Instantiate, т. е. при первом его появлении с помощью системы пулинга
    }

    public virtual void OnSpawnedFromPool()
    {
        //для переопределения наследуемыми классами, вызывается при создании или повторном использовании объекта из пула
        //здесь рекомендуется восстановить значения переменных и другие параметры в исходное состояние
        //при создании объекта вызывается сразу после Initialize()
    }

    public virtual void OnReturnedToPool()
    {
        //для переопределения наследуемыми классами, вызывается при возвращении объекта в пул
    }
}
