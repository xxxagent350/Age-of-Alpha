using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setTargetFPS : MonoBehaviour
{

    [SerializeField] int targetFPS = 60;

    private void Start()
    {
        SlowUpdate();
    }

    void SlowUpdate()
    {
        Application.targetFrameRate = targetFPS;
        Invoke(nameof(SlowUpdate), 3);
    }

}
