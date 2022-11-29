using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMove : MonoBehaviour
{

    public float timeSpeed = 1f;

    public Transform posA;
    public Transform posB;

    private float oldTime;
    void Update ()
    {
        float time = Mathf.PingPong (Time.time * timeSpeed, 1f);
        time = Mathf.SmoothStep (0f, 1f, time);
        oldTime = time;
        Vector3 newPos = Vector3.Lerp (posA.position, posB.position, time);
        transform.position = newPos;

    }

}