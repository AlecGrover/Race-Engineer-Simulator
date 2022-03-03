using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MediumTires : Tires
{
    public int ExpectedLifetime { get; private set; } = 12;

    public float GetPaceModifier(float lapCount)
    {
        float paceModifier= 0.035f * Mathf.Pow(lapCount, 2) - 0.1f * lapCount - 1;
        return paceModifier;
    }

    public float GetIntegralBetweenLaps(float start, float end)
    {
        float timeModifier = (0.035f/3f * Mathf.Pow(end, 3)) - (0.035f/3f * Mathf.Pow(start, 3));
        return timeModifier;
    }

    public TireType GetTireType()
    {
        return TireType.Medium;
    }
}
