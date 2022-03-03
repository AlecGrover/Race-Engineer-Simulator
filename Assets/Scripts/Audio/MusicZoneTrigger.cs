using System;
using UnityEngine;

namespace Audio
{
    public class MusicZoneTrigger : MonoBehaviour
    {

        public MusicController musicController;
        public AudioClip musicClip;
        [Range(0f, 1f)]
        public float songVolumeModifier = 1f;
        private int _hits = 0;

        public void OnTriggerEnter(Collider other)
        {
            _hits++;
            if (_hits == 1) musicController.QueueSong(musicClip, songVolumeModifier);
        }

        public void OnTriggerExit(Collider other)
        {
            _hits--;
            if (_hits > 0) return;
            _hits = 0;
            musicController.LeaveZone();
        }
        
    }
}
