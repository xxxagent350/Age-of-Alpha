using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddRandomForce : MonoBehaviour
{

    [SerializeField] float timeToAddForce = 5;
    float timerToAddForce;
    [SerializeField] float forceAmplitude;
    [SerializeField] float rotForceAmplitude;
    Rigidbody2D body;

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        timerToAddForce += Time.deltaTime;
        if (timerToAddForce > timeToAddForce)
        {
            timerToAddForce = 0;
            body.AddRelativeForce(new Vector2(0, Random.Range(0, forceAmplitude)));
            body.AddTorque(Random.Range(-rotForceAmplitude, rotForceAmplitude));
        }
    }

}
