using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISpinner : MonoBehaviour
{

    public float SpinsPerSecond = 1;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, 0, 2 * 360 * SpinsPerSecond * Time.deltaTime), Space.Self);
    }
}
