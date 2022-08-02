using UnityEngine;

namespace Hauler.Audio {
    public class SwitchClipAudioController : BaseAudioController {
        public Vector2 pitchRange = new Vector2(0.8f, 1.2f);
        public AudioClip clipState0, clipState1;

        bool state;

        public override void PlaySound() {
            audioSource.clip = state ? clipState1 : clipState0;
            audioSource.pitch = Mathf.Lerp(pitchRange.x, pitchRange.y, Random.value);
            audioSource.Play();
        }

        public void SetState(bool state) {
            this.state = state;
        }
    }
}
