using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Track : ScriptableObject
{
    public float Sector1Length= 10f;
    public float Sector2Length= 10f;
    public float Sector3Length= 10f;


    public Sprite Sector1Sprite;
    public Sprite Sector2Sprite;
    public Sprite Sector3Sprite;
    public Sprite NoSectorSprite;

    [Range(1, 100)]
    public int LapCount = 30;

    public AudioClip Soundtrack;


    public int GetSectorNumber(float rawPosition)
    {
        rawPosition -= Mathf.Floor(rawPosition);
        float totalLength = GetLapTime();
        int currentSector;
        if (rawPosition < Sector1Length / totalLength)
        {
            currentSector= 1;
        }
        else if (rawPosition < (Sector1Length + Sector2Length) / totalLength)
        {
            currentSector= 2;
        }
        else
        {
            currentSector= 3;
        }

        return currentSector;

    }

    public float GetLapTime()
    {
        float fTotalLength= Sector1Length + Sector2Length + Sector3Length;
        return fTotalLength;
    }


    public float GetLapCount()
    {
        return LapCount;
    }
}
