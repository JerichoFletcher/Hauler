using UnityEngine;
using UnityEngine.Events;

namespace Hauler.Entity.Interactable {
    public class CallEventInteractable : BaseInteractable {
        public bool invokeState;
        public float minInvokeInterval = .6f;
        public UnityEvent<bool>[] events;

        float lastInvokeTime;

        public override void Act(UnityEvent<bool> success) {
            if(Time.time >= lastInvokeTime + minInvokeInterval) {
                audioController?.PlaySound();
                foreach(UnityEvent<bool> callable in events) {
                    callable.Invoke(invokeState);
                }
                lastInvokeTime = Time.time;
            }
        }
    }
}
