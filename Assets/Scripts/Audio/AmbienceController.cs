using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
    public class AmbienceController : MonoBehaviour
    {

        public AudioMixerGroup MixerGroup;

        public AudioClip AmbientTrack;
        [Range(0f, 1f)]
        public float Volume = 0.5f;

        public bool UseParentPosition = true;
        public Vector3 AmbientSource = Vector3.zero;
        [Range(1e-36f, float.PositiveInfinity)]
        public float AmbientRange = 10f;
        [Range(0, 1f)]
        public float FalloffPercent = 0.8f;
        private AudioSource _audioSource;

        // Start is called before the first frame update
        private void Start()
        {
            // InitializeCollider();
            if (!UseParentPosition) gameObject.transform.position = AmbientSource;
            InitializeAudioSource();
        }

        private void InitializeAudioSource()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = AmbientTrack;
            _audioSource.outputAudioMixerGroup = MixerGroup;
            _audioSource.rolloffMode = AudioRolloffMode.Linear;
            UpdateVolumeParameters();
            _audioSource.loop = true;
            _audioSource.spatialBlend = 1f;
            _audioSource.spread = 180;
            _audioSource.Play();
        }

        private void UpdateVolumeParameters()
        {
            _audioSource.volume = Volume;
            _audioSource.minDistance = AmbientRange * FalloffPercent;
            _audioSource.maxDistance = AmbientRange;
        }

        private void OnDrawGizmosSelected()
        {
            var center = UseParentPosition ? transform.position : AmbientSource;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(center, AmbientRange);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(center, AmbientRange * FalloffPercent);
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            if (Debug.isDebugBuild)
            {
                UpdateVolumeParameters();
            }
        }
    }
}
