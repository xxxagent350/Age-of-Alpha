using UnityEngine;

public abstract class PooledBehaviour : MonoBehaviour
{
    public virtual void Initialize()
    {
        //���������� ��� �������� ������� ��� ������ Instantiate, �. �. ��� ������ ��� ��������� � ������� ������� �������
    }

    public virtual void OnSpawnedFromPool()
    {
        //��� ��������������� ������������ ��������, ���������� ��� �������� ��� ��������� ������������� ������� �� ����
        //����� ������������� ������������ �������� ���������� � ������ ��������� � �������� ���������
        //��� �������� ������� ���������� ����� ����� Initialize()
    }

    public virtual void OnReturnedToPool()
    {
        //��� ��������������� ������������ ��������, ���������� ��� ����������� ������� � ���
    }
}
