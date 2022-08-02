using UnityEngine;

namespace Hauler.World {
    public class LinkedDoorBehavior : DoorBehaviour {
        public LinkedDoorBehavior linkedDoors;
        public bool inverseLink, linkOnlyWhenClosing;

        public override void ToggleState() {
            base.ToggleState();
            if(linkedDoors != null && (!linkOnlyWhenClosing || !currentState)) {
                linkedDoors.SetState(inverseLink ? !currentState : currentState);
            }
        }
    }
}
