using UnityEngine;
using TMPro;

public class ScrollingContentExpanderWithUIText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    void Update()
    {
        text.transform.parent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, text.renderedHeight + 10);
    }
}
