using UnityEngine;
using Hauler.Audio;

namespace Hauler.World {
    [RequireComponent(typeof(Animator))]
    public class DoorBehaviour : MonoBehaviour {
        public string stateFieldName;
        public bool initState;

        protected Animator animator;
        protected BaseAudioController audioController;
        protected bool currentState;

        private void Awake() {
            if(!TryGetComponent(out audioController))
                Debug.LogWarning($"No audio controller added to {gameObject.name}");

            animator = GetComponent<Animator>();
            SetState(initState, false);
        }

        public virtual void ToggleState() {
            SetState(!currentState);
        }

        public virtual void SetState(bool newState, bool playsound = true) {
            bool stateChanged = currentState != newState;
            currentState = newState;
            if(audioController is SwitchClipAudioController audioSwitch) {
                audioSwitch.SetState(newState);
            }
            animator.SetBool(stateFieldName, currentState);
            if(playsound && stateChanged) audioController?.PlaySound();
        }
    }
}
