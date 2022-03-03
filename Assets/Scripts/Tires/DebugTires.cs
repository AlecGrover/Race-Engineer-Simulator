using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DebugTires : Tires
{

    public int ExpectedLifetime { get; private set;  } = 3;
    public float GetPaceModifier(float lapCount)
    {
        float paceModifier;
        if (lapCount < 2)
        {
            paceModifier = -5f;
        }
        else if (lapCount >= 3f)
        {
            paceModifier = 20f;
        }
        else
        {
            paceModifier = 0;
        }

        return paceModifier;
    }

    public float GetIntegralBetweenLaps(float start, float end)
    {
        return 150f;
    }

    public TireType GetTireType()
    {
        return TireType.Debug;
    }
}
