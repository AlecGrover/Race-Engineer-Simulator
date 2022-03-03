using System.Collections;
using System.Collections.Generic;
using Audio;
using UnityEngine;

public class SceneSoundtrackTrigger : MonoBehaviour
{

    // Locates a MusicController and queues the scene's soundtrack. Essentially an alternative to A Drop of Magic's music zone trigger objects

    public AudioClip SoundtrackAudio;


    // Start is called before the first frame update
    void Start()
    {
        MusicController musicController = FindObjectOfType<MusicController>();
        if (musicController)
        {
            Debug.Log("Queuing Song");
            musicController.QueueSong(SoundtrackAudio);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
