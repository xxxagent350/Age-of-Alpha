using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PauseInterfaceManager : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] private RectTransform backgroundRectTransform;
    [SerializeField] private float animationsSpeed = 5;
    [SerializeField] private List<Image> buttonsImages;

    [Header("Отладка")]
    [SerializeField] private PauseInterfaceState pauseInterfaceState;

    private Vector3 startBackgroundRectTransformLocalscale;
    private float _animationProgress;

    private void Awake()
    {
        startBackgroundRectTransformLocalscale = backgroundRectTransform.localScale;
        ApplyAnimationProgress(0);
    }

    public void EnableInterface()
    {
        gameObject.SetActive(true);
        pauseInterfaceState = PauseInterfaceState.enabling;
    }

    public void DisableInterface()
    {
        pauseInterfaceState = PauseInterfaceState.disabling;
    }

    private void Update()
    {
        if (pauseInterfaceState == PauseInterfaceState.enabling)
        {
            if (_animationProgress < 1)
            {
                _animationProgress += Time.deltaTime * animationsSpeed;
            }
            else
            {
                _animationProgress = 1;
                pauseInterfaceState = PauseInterfaceState.waiting;
            }
        }
        if (pauseInterfaceState == PauseInterfaceState.disabling)
        {
            if (_animationProgress > 0)
            {
                _animationProgress -= Time.deltaTime * animationsSpeed;
            }
            else
            {
                _animationProgress = 0;
                pauseInterfaceState = PauseInterfaceState.waiting;
                gameObject.SetActive(false);
            }
        }

        ApplyAnimationProgress(_animationProgress);
    }

    private void ApplyAnimationProgress(float animationProgress)
    {
        backgroundRectTransform.localScale = new Vector3(backgroundRectTransform.localScale.x, startBackgroundRectTransformLocalscale.y * animationProgress, backgroundRectTransform.localScale.z);

        foreach (Image buttonImage in buttonsImages)
        {
            Color oldColor = buttonImage.color;
            buttonImage.color = new Color(oldColor.r, oldColor.g, oldColor.b, animationProgress);
        }
    }


    enum PauseInterfaceState
    {
        waiting,
        enabling,
        disabling
    }
}
