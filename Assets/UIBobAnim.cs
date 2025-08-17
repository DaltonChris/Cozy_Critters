using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBobAnim : MonoBehaviour
{
    public float bobAmp = 2f;
    public float frequency = 1f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float newY = Mathf.Sin(Time.time * frequency) * bobAmp;
        transform.localPosition = startPos + new Vector3(0, newY, 0);
    }
}

