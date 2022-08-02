using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hauler.Util;

namespace Hauler.AI.Pathfinding {
    // Thanks to Sebastian Lague for providing a very in-depth tutorial series on this
    [RequireComponent(typeof(PathGrid))]
    public class AstarPathfinding : MonoBehaviour {
        PathGrid grid;
        Heap<PathNode> openSet;
        HashSet<PathNode> closedSet = new HashSet<PathNode>();

        List<PathNode> pathTemp = new List<PathNode>();
        List<Vector2> waypointsTemp = new List<Vector2>();

        PathRequestManager requestManager;

        public AstarPathfinding Instance { get; private set; }

        void Awake() {
            Instance = this;
            grid = GetComponent<PathGrid>();
            requestManager = GetComponent<PathRequestManager>();
        }

        void Start() {
            openSet = new Heap<PathNode>(grid.NodeCount);
        }

        public void StartFindPath(Vector2 start, Vector2 end) {
            StopAllCoroutines();
            StartCoroutine(FindPath(start, end));
        }

        IEnumerator FindPath(Vector2 start, Vector2 end) {
            // Initialization
            Vector2[] waypoints = new Vector2[0];
            bool success = false;

            PathNode startNode = grid.NodeFromWorldPos(start), endNode = grid.NodeFromWorldPos(end);
            if(startNode.Walkable && endNode.Walkable) {
                openSet.Clear();
                closedSet.Clear();

                openSet.Add(startNode);
                // Keep looping until there are no more nodes available
                while(openSet.Count > 0) {
                    // Find the node in openSet with the smallest FCost (and HCost if there is more than one)
                    PathNode current = openSet.RemoveFirst();
                    closedSet.Add(current);

                    // Bail when the end node is reached
                    if(current == endNode) {
                        success = true;
                        break;
                    }

                    foreach(PathNode neighbor in grid.NeighborsOf(current)) {
                        // Don't evaluate untraversable or already evaluated neighboring nodes
                        if(!neighbor.Walkable || closedSet.Contains(neighbor)) continue;

                        // Calculate neighbor's GCost and overwrite its stored GCost if this path is better
                        int moveCost = current.GCost + DistanceBetween(current, neighbor);
                        if(moveCost < neighbor.GCost || !openSet.Contains(neighbor)) {
                            neighbor.GCost = moveCost;
                            neighbor.HCost = DistanceBetween(neighbor, endNode);
                            neighbor.Parent = current;
                            // Add the node to openSet for evaluation
                            if(!openSet.Contains(neighbor)) openSet.Add(neighbor);
                        }
                    }
                }
            }
            // No path was found if the code reached this point
            if(success) waypoints = RetracePath(startNode, endNode);
            yield return null;
            requestManager?.FinishedProcessing(waypoints, success);
        }

        void Update() {
            //FindPath(seeker.position, target.position);
        }

        Vector2[] RetracePath(PathNode start, PathNode end) {
            pathTemp.Clear();
            PathNode current = end;
            while(current != start) {
                pathTemp.Add(current);
                current = current.Parent;
            }
            if(current == start) pathTemp.Add(current);
            Vector2[] waypoints = SimplifyPath(pathTemp);
            Array.Reverse(waypoints);
            return waypoints;
        }

        Vector2[] SimplifyPath(List<PathNode> path) {
            waypointsTemp.Clear();
            Vector2 direction = Vector2.zero;
            for(int i = 1; i < path.Count; i++) {
                Vector2 newDir = path[i - 1].GridPosition - path[i].GridPosition;
                if(newDir != direction) waypointsTemp.Add(path[i - 1].WorldPosition);
                direction = newDir;
            }
            return waypointsTemp.ToArray();
        }

        int DistanceBetween(PathNode a, PathNode b) {
            Vector2Int dist = b.GridPosition - a.GridPosition;
            dist.x = Mathf.Abs(dist.x);
            dist.y = Mathf.Abs(dist.y);

            // A horizontal/vertical step is worth 10 units, whereas a diagonal step is worth 14 units
            if(dist.x < dist.y) {
                return 14 * dist.x + 10 * (dist.y - dist.x);
            } else {
                return 14 * dist.y + 10 * (dist.x - dist.y);
            }
        }
    }
}
