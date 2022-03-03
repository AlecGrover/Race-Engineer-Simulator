using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using Random = System.Random;

namespace Audio
{
    public class MusicController : MonoBehaviour
    {

        // Re-purposed system from A Drop of Magic
        // TODO: Return and comment

        public AudioClip[] defaultSongs;
        private AudioClip _currentSong;
        [Range(0f, 1f)]
        public float volume = 0.5f;
        private float _songVolume;
        public float fadeTime = 1.5f;
        public int minimumDuration = 120;
        public int maximumDuration = 300;
        public AudioMixerGroup audioMixerGroup;

        private List<AudioSource> _audioSources = new List<AudioSource>();

        private void Awake()
        {
            // DontDestroyOnLoad(gameObject);
            MusicController[] musicControllers = FindObjectsOfType<MusicController>();
            if (musicControllers.Length > 1)
            {
                Destroy(gameObject);
            }
            StartSong(defaultSongs[new Random().Next(defaultSongs.Length)]);
        }

        private void StartSong(AudioClip currentSong, bool defaultSong = true, float songVolume = 1f)
        {
            _currentSong = currentSong;
            var newAudioSource = gameObject.AddComponent<AudioSource>();
            _songVolume = songVolume; 
            newAudioSource.clip = currentSong;
            newAudioSource.volume = 0;
            newAudioSource.loop = true;
            newAudioSource.outputAudioMixerGroup = audioMixerGroup;
            newAudioSource.Play();
            StopAllCoroutines();
            foreach (var audioSource in _audioSources)
            {
                StartCoroutine(FadeOut(audioSource));
            }
            _audioSources.Add(newAudioSource);
            StartCoroutine(FadeIn(newAudioSource));
            if (defaultSong) StartCoroutine(RandomSong());
        }

        private IEnumerator FadeIn(AudioSource audioSource)
        {
            var initialVolume = audioSource.volume;
            for (var i = (Mathf.RoundToInt(initialVolume/volume) * 100); i <= 100; i++)
            {
                audioSource.volume = (i / 100f) * volume * _songVolume;
                yield return new WaitForSeconds(fadeTime/100f);
            }
        }

        private IEnumerator FadeOut(AudioSource audioSource)
        {
            var initialVolume = audioSource.volume;
            for (var i = (Mathf.RoundToInt(initialVolume/volume) * 100); i >= 0; i--)
            {
                audioSource.volume = initialVolume * (i / 100f);
                yield return new WaitForSeconds(fadeTime / 100f);
            }
            _audioSources.Remove(audioSource);
            Destroy(audioSource);
            yield return null;
        }

        public void LeaveZone()
        {
            StartSong(defaultSongs[new Random().Next(defaultSongs.Length)]);
        }

        private IEnumerator RandomSong()
        {
            yield return new WaitForSeconds(new Random().Next(minimumDuration, maximumDuration));
            AudioClip nextSong;
            // TODO: Fix this statistically impossible infinite loop chance
            if (defaultSongs.Length > 1)
            {
                nextSong = defaultSongs.ToList().Where(song => song != _currentSong).ToArray()[new Random().Next(defaultSongs.Length - 1)];
            }
            else
            {
                nextSong = _currentSong;
            }
            StartSong(nextSong);
        }


        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void QueueSong(AudioClip musicClip, float volumeMod = 1f)
        {
            Debug.Log("Queued Song Received");
            StartSong(musicClip, false, volumeMod);
        }
    }
}
