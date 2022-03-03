using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardTires : Tires
{
    public int ExpectedLifetime { get; private set; } = 18;

    public float GetPaceModifier(float lapCount)
    {
        float paceModifier = 0.01f * Mathf.Pow(lapCount, 2);
        return paceModifier;
    }

    public float GetIntegralBetweenLaps(float start, float end)
    {
        float timeModifier = (0.01f/3f * Mathf.Pow(end, 3)) - (0.01f/3 * Mathf.Pow(start, 3));
        return timeModifier;
    }

    public TireType GetTireType()
    {
        return TireType.Hard;
    }
}
