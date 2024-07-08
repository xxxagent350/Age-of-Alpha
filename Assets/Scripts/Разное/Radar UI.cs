using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RadarUI : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] private Radar _radar;

    [SerializeField] private RectTransform _scanner;
    [SerializeField] private float _scannerRotateSpeed = 0.38f;

    [SerializeField] private List<Image> _gridImages;
    [Tooltip("Сколько метров будет означать одна ячейка сетки на радаре")]
    [SerializeField] private float _metresInGridCell = 100;

    private void Update()
    {
        VisualizeScanner();
        VisualizeGrid();
    }

    private void VisualizeScanner()
    {
        _scanner.localEulerAngles += new Vector3(0, 0, _scannerRotateSpeed * -360 * Time.deltaTime);
    }

    private void VisualizeGrid()
    {
        float pixelsPerUnit = _radar.MapScale / _metresInGridCell;
        foreach (Image gridImage in _gridImages)
        {
            gridImage.pixelsPerUnitMultiplier = pixelsPerUnit;
            gridImage.SetAllDirty();
        }
    }
}
