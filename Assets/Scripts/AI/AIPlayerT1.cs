using UnityEngine;

public class AIPlayerT1 : MonoBehaviour
{
    [Header("���������")]
    [Tooltip("��� ��������� ����: None - ������������, Simple - ����� ����� ������ �� ���������� ���������� � �������")]
    public BehaviourModes behaviourMode;
    [Tooltip("���������� ������ ���������. ����� �������� ������")]
    public AIParametresT1 parametresPreset;
    [Tooltip("������� ��������� ���������")]
    public AIParametresT1Struct currentParametres;

    [Header("�������")]
    [SerializeField] private ShipGameStats target;

    private void FixedUpdate()
    {
        
    }

    public enum BehaviourModes
    {
        None,
        Sipmle
    }
}
