using UnityEngine;
using TMPro;

public class FPSshower : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _fpsShowerText;

    private float _lowestFPSDetected = -1;
    private float _currentLowestFPS;
    private float _timerToResetLowestFPS;

    private void Update()
    {
        float currentFPS = Mathf.RoundToInt(1 / Time.deltaTime);
        if (_fpsShowerText != null)
        {
            _fpsShowerText.text = $"FPS: {currentFPS}\nTARGET: {Application.targetFrameRate}\nLOWEST: {_lowestFPSDetected}";
        }


        _timerToResetLowestFPS += Time.deltaTime;
        if (_timerToResetLowestFPS >= 1)
        {
            _timerToResetLowestFPS = 0;
            _lowestFPSDetected = _currentLowestFPS;
            _currentLowestFPS = 99999;
        }

        if (currentFPS < _currentLowestFPS)
        {
            _currentLowestFPS = currentFPS;
        }
    }
}
