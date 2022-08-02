using UnityEngine;

namespace Hauler.Audio {
    [RequireComponent(typeof(AudioSource))]
    public abstract class BaseAudioController : MonoBehaviour {
        protected AudioSource audioSource;

        protected virtual void Awake() {
            audioSource = GetComponent<AudioSource>();
        }

        public abstract void PlaySound();
    }
}
