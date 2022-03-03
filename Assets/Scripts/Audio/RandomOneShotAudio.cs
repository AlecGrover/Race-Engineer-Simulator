using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu]
[Serializable]
public class RandomOneShotAudio : ScriptableObject
{

    // Holds a set of audio clips and returns one at random for use as one shot audio when asked for


    public AudioClip[] AudioClips = new AudioClip[0];

    public AudioClip GetRanAudioClipFromList()
    {
        if (AudioClips.Length == 0) return null;
        int randomIndex = Mathf.FloorToInt(Random.Range(0, maxExclusive:AudioClips.Length));
        return AudioClips[randomIndex];
    }

}
