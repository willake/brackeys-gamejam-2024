using UnityEngine;

namespace Game
{
    [System.Serializable]
    public class WrappedAudioClip
    {
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1;
    }
}