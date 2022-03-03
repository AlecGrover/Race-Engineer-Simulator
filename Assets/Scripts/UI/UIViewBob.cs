using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIViewBob : MonoBehaviour
{

    public float BobHeight = 5f;
    public float BobsPerSecond = 1;

    void Start()
    {
        transform.localPosition -= new Vector3(0, BobHeight / 2, 0);
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition += new Vector3(0, BobHeight * Time.deltaTime * Mathf.Sin(2*Mathf.PI * Time.time), 0);
    }
}
