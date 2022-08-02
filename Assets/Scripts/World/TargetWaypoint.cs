using UnityEngine;
using Hauler.Util;

namespace Hauler.World {
    public class TargetWaypoint : MonoBehaviour {
        public WeightedElement<TargetWaypoint>[] connections;

        public TargetWaypoint Select() {
            return WeightedElement<TargetWaypoint>.Select(connections);
        }
    }
}
