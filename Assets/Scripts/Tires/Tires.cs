using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Tires
{
    public int ExpectedLifetime { get; }
    public float GetPaceModifier(float lapCount);
    public float GetIntegralBetweenLaps(float start, float end);
    public TireType GetTireType();
}
