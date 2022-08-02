using UnityEngine;
using UnityEngine.Events;
using Hauler.Audio;

namespace Hauler.Entity.Interactable {
    [RequireComponent(typeof(Collider2D))]
    public abstract class BaseInteractable : MonoBehaviour {
        new protected Collider2D collider;
        protected BaseAudioController audioController;

        private void Awake() {
            collider = GetComponent<Collider2D>();
            if(!TryGetComponent(out audioController))
                Debug.LogWarning($"No audio controller added to {gameObject.name}");
        }

        public virtual void Act() {
            Act(null);
        }

        public abstract void Act(UnityEvent<bool> success);
    }
}
