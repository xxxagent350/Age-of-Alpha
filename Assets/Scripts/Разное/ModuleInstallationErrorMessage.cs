using UnityEngine;
using TMPro;

public class ModuleInstallationErrorMessage : MonoBehaviour
{
    [Header("���������")]
    [SerializeField] float showedTime; //����� ����������� ������
    [SerializeField] float blinkingTime; //����� �������� ������
    [SerializeField] float blinkingInterval; //�������� �������� ������
    [SerializeField] AudioClip errorSound; //���� ��� ����������� ������
    [SerializeField] float errorSoundVolume;

    [Header("�������")]
    [SerializeField] float showedTimer = 0; //������ ����������� ������
    [SerializeField] float blinkingIntervalTimer = 0; //������ �������� ������
    [SerializeField] bool showedByBlinking = true; //������� �� ����� ��-�� ��������
    [SerializeField] TextMeshProUGUI textComponent;

    private void Start()
    {
        showedTimer = showedTime + 1;
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        showedTimer += Time.deltaTime;
        blinkingIntervalTimer += Time.deltaTime;

        if (showedTimer < showedTime) //����� �������
        {
            if (showedTimer < blinkingTime || !showedByBlinking) //����� �������
            {
                if (showedByBlinking) //����� �������
                {
                    textComponent.enabled = true;
                    if (blinkingIntervalTimer > blinkingInterval)
                    {
                        blinkingIntervalTimer = 0;
                        showedByBlinking = false;
                    }
                }
                else //����� �� �������
                {
                    textComponent.enabled = false;
                    if (blinkingIntervalTimer > blinkingInterval)
                    {
                        blinkingIntervalTimer = 0;
                        showedByBlinking = true;
                    }
                }
            }
            else //����� �� �������
            {
                textComponent.enabled = true;
            }
        }
        else //����� �� �������
        {
            textComponent.enabled = false;
        }
    }

    public void ShowErrorMessage(string message)
    {
        textComponent.text = message;
        showedTimer = 0;
        blinkingIntervalTimer = 0;
        showedByBlinking = true;
        if (errorSound != null)
        {
            DataOperator.instance.PlayUISound(errorSound, errorSoundVolume);
        }
    }
}
