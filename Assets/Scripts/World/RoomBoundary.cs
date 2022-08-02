using UnityEngine;

namespace Hauler.World {
    [RequireComponent(typeof(Collider2D))]
    public class RoomBoundary : MonoBehaviour {
        public RoomType type;
        public TargetWaypoint[] waypointsInRoom;

        public bool IsVent => (int)type > 15;
    }

    public enum RoomType {
        CommandModule = 0,
        ServiceModule = 1,
        CMLeftVentShaft = 16,
        CMRightVentShaft = 17,
    }
}
