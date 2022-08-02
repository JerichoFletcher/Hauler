using UnityEngine;

namespace Hauler.Audio {
    public class RandomClipAudioController : BaseAudioController {
        public Vector2 pitchRange = new Vector2(0.8f, 1.2f);
        public AudioClip[] audioClips;

        protected override void Awake() {
            base.Awake();
            if(audioClips != null && audioClips.Length > 0) {
                audioSource.clip = audioClips[0];
            } else {
                Debug.LogWarning($"No audio clips assigned to random clip audio controller on {gameObject.name}");
            }
        }

        public override void PlaySound() {
            audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
            audioSource.pitch = Mathf.Lerp(pitchRange.x, pitchRange.y, Random.value);
            audioSource.Play();
        }
    }
}
