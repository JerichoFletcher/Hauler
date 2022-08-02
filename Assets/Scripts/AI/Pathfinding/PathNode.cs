using UnityEngine;
using Hauler.Util;

namespace Hauler.AI.Pathfinding {
    // Thanks to Sebastian Lague for providing a very in-depth tutorial series on this
    public class PathNode : IHeapItem<PathNode> {
        public PathNode Parent { get; set; }
        public bool Walkable { get; set; }
        public Vector2 WorldPosition { get; }
        public Vector2Int GridPosition { get; }

        public int FCost => GCost + HCost;      // Sum of GCost and HCost
        public int GCost { get; set; }          // Path distance to the start node
        public int HCost { get; set; }          // Grid distance to the end node
        public int HeapIndex { get => heapIndex; set => heapIndex = value; }

        int heapIndex;

        public PathNode(bool walkable, Vector2 worldPos, Vector2Int gridPos) {
            Walkable = walkable;
            WorldPosition = worldPos;
            GridPosition = gridPos;
        }

        public int CompareTo(PathNode other) {
            int compare = FCost.CompareTo(other.FCost);
            if(compare == 0) compare = HCost.CompareTo(other.HCost);
            return -compare;
        }
    }
}
