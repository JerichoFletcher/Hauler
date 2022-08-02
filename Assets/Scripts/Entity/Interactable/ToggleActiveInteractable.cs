using UnityEngine;
using UnityEngine.Events;

namespace Hauler.Entity.Interactable {
    public class ToggleActiveInteractable : BaseInteractable {
        public bool startActive;
        public GameObject[] targets;

        bool active;

        private void Start() {
            SetState(startActive);
        }

        public override void Act(UnityEvent<bool> success) {
            audioController?.PlaySound();
            SetState(!active);
            success?.Invoke(active);
        }

        void SetState(bool newState) {
            active = newState;
            foreach(GameObject b in targets)
                b.SetActive(active);
        }
    }
}
