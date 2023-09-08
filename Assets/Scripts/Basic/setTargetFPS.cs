using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class setTargetFPS : MonoBehaviour
{

    private void Start()
    {
        Application.targetFrameRate = 60;
    }

}
