using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoftTires : Tires
{
    public int ExpectedLifetime { get; private set; } = 6;

    public float GetPaceModifier(float lapCount)
    {
        float paceModifier= 0.15f * Mathf.Pow(lapCount, 2) - 5;
        return paceModifier;
    }

    public float GetIntegralBetweenLaps(float start, float end)
    {
        float timeModifier = (0.05f * Mathf.Pow(end, 3)) - (0.05f * Mathf.Pow(start, 3));
        return timeModifier;
    }

    public TireType GetTireType()
    {
        return TireType.Soft;
    }
}
