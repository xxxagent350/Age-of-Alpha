using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextTranslator : MonoBehaviour
{
    string translate;

    [SerializeField] string russian;
    [SerializeField] string english;

    Text textUI;
    TextMeshProUGUI textMeshProUI;
    TextMeshPro textMeshPro;

    private void Start()
    {
        if (DataOperator.instance != null)
            Translate();
        else
            Invoke(nameof(Translate), 0.05f);
    }

    void Translate()
    {
        //���������
        if (DataOperator.instance.userLanguage == "Russian")
            translate = russian;
        if (DataOperator.instance.userLanguage == "English")
            translate = english;

        //�������� ��������� ���������� ������
        textUI = GetComponent<Text>();
        textMeshProUI = GetComponent<TextMeshProUGUI>();
        textMeshPro = GetComponent<TextMeshPro>();

        //������� ����������� ������ ������� ���� ��� ����
        if (textUI != null)
        {
            textUI.text = translate;
        }
        if (textMeshProUI != null)
        {
            textMeshProUI.text = translate;
        }
        if (textMeshPro != null)
        {
            textMeshPro.text = translate;
        }
    }
}
