using UnityEngine;

namespace Hauler.Entity.Camera {
    public class CameraFollow : MonoBehaviour {
        public Transform target;
        public float transitionSpeed = 0.8f;

        private void LateUpdate() {
            float z = transform.position.z;
            Vector3 newPosition = Vector3.Lerp(transform.position, target.position, transitionSpeed);
            newPosition.z = z;
            transform.position = newPosition;
        }
    }
}
