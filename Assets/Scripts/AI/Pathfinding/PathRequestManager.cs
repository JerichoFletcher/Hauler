using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hauler.AI.Pathfinding {
    // Thanks to Sebastian Lague for providing a very in-depth tutorial series on this
    [RequireComponent(typeof(AstarPathfinding))]
    public class PathRequestManager : MonoBehaviour {
        Queue<PathRequest> requestQueue = new Queue<PathRequest>();
        PathRequest currentRequest;

        AstarPathfinding pathfinding;
        bool isProcessing;

        public static PathRequestManager Instance { get; private set; }

        void Awake() {
            Instance = this;
            pathfinding = GetComponent<AstarPathfinding>();
        }

        public static void RequestPath(Vector2 start, Vector2 end, Action<Vector2[], bool> callback) {
            PathRequest newRequest = new PathRequest(start, end, callback);
            Instance.requestQueue.Enqueue(newRequest);
            Instance.TryProcessNext();
        }

        public void FinishedProcessing(Vector2[] path, bool success) {
            currentRequest.Callback(path, success);
            isProcessing = false;
            TryProcessNext();
        }

        void TryProcessNext() {
            if(!isProcessing && requestQueue.Count > 0) {
                currentRequest = requestQueue.Dequeue();
                isProcessing = true;
                pathfinding.StartFindPath(currentRequest.Start, currentRequest.End);
            }
        }

        struct PathRequest {
            public Vector2 Start { get; }
            public Vector2 End { get; }
            public Action<Vector2[], bool> Callback { get; }

            public PathRequest(Vector2 start, Vector2 end, Action<Vector2[], bool> callback) {
                Start = start;
                End = end;
                Callback = callback;
            }
        }
    }
}
