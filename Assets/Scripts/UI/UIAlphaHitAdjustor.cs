using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIAlphaHitAdjustor : MonoBehaviour
{
    [Range(0f, 1.0f)]
    public float AlphaMinimum = 0.4f;

    void Start()
    {
        var image = GetComponent<Image>();
        image.alphaHitTestMinimumThreshold = AlphaMinimum;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
