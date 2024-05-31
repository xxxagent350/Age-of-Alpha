using UnityEngine;

public abstract class PooledBehaviour : MonoBehaviour
{
    public virtual void OnSpawnedFromPool()
    {
        //��� ��������������� ������������ ��������, ���������� ��� �������� ��� ��������� ������������� ������� �� ����
        //����� ������������� ������������ �������� ���������� � ������ ��������� � �������� ���������
    }

    public virtual void OnReturnedToPool()
    {
        //��� ��������������� ������������ ��������, ���������� ��� ����������� ������� � ���
    }
}
