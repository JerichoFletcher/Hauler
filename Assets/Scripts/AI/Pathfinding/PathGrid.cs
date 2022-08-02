using System.Collections.Generic;
using UnityEngine;

namespace Hauler.AI.Pathfinding {
    // Thanks to Sebastian Lague for providing a very in-depth tutorial series on this
    public class PathGrid : MonoBehaviour {
        public bool drawGizmos;
        public Vector2 gridWorldSize;
        public float nodeRadius = .5f;
        public LayerMask obstacleMask;

        public float refreshInterval = 1f;

        public float nodeDiameter => nodeRadius * 2f;
        public int NodeCount => gridSize.x * gridSize.y;

        PathNode[,] grid;
        List<PathNode> tempList = new List<PathNode>();
        Vector2Int gridSize;
        Vector2 worldBL;
        float lastRefresh;

        void Awake() {
            gridSize = new Vector2Int(Mathf.RoundToInt(gridWorldSize.x / nodeDiameter), Mathf.RoundToInt(gridWorldSize.y / nodeDiameter));
            CreateGrid();
        }

        void Update() {
            if(Time.time > lastRefresh + refreshInterval) {
                foreach(PathNode node in grid) {
                    node.Walkable = !Physics2D.OverlapCircle(node.WorldPosition, nodeRadius / 2f, obstacleMask);
                }
                lastRefresh = Time.time;
            }
        }

        public PathNode NodeFromWorldPos(Vector2 worldPos) {
            Vector2 relPos = new Vector2(Mathf.Clamp01((worldPos.x - worldBL.x) / gridWorldSize.x), Mathf.Clamp01((worldPos.y - worldBL.y) / gridWorldSize.y));
            Vector2Int gridPos = new Vector2Int(Mathf.CeilToInt(gridSize.x * relPos.x) - 1, Mathf.CeilToInt(gridSize.y * relPos.y) - 1);
            return grid[gridPos.x, gridPos.y];
        }

        public List<PathNode> NeighborsOf(PathNode node) {
            tempList.Clear();
            // Loop through each neighbors in 3x3 area
            for(int x = -1; x <= 1; x++) {
                for(int y = -1; y <= 1; y++) {
                    if(x == 0 && y == 0) continue;
                    Vector2Int check = node.GridPosition + new Vector2Int(x, y);
                    if(check.x >= 0 && check.x < gridSize.x && check.y >= 0 && check.y < gridSize.y) {
                        tempList.Add(grid[check.x, check.y]);
                    }
                }
            }
            return tempList;
        }

        void CreateGrid() {
            grid = new PathNode[gridSize.x, gridSize.y];
            worldBL = transform.position + Vector3.left * gridWorldSize.x / 2f + Vector3.down * gridWorldSize.y / 2f;

            // Loop through the grid
            for(int x = 0; x < gridSize.x; x++) {
                for(int y = 0; y < gridSize.y; y++) {
                    // worldPos is the center of the current node
                    Vector2 worldPos = worldBL + Vector2.right * (x * nodeDiameter + nodeRadius) + Vector2.up * (y * nodeDiameter + nodeRadius);

                    // Node is walkable if there is no obstacle within nodeRadius from its center
                    // For some reason OverlapCircle treats radius as diameter, which is kind of annoying
                    bool walkable = !Physics2D.OverlapCircle(worldPos, nodeRadius / 2f, obstacleMask);
                    grid[x, y] = new PathNode(walkable, worldPos, new Vector2Int(x, y));
                }
            }
        }

        void OnDrawGizmos() {
            Gizmos.DrawWireCube(transform.position, gridWorldSize);
            if(grid != null && drawGizmos) {
                foreach(PathNode node in grid) {
                    Gizmos.color = node.Walkable ? Color.white : Color.red;
                    Gizmos.DrawCube(node.WorldPosition, Vector2.one * (nodeDiameter - .1f));
                }
            }
        }
    }
}
