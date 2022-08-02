using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hauler.Audio;
using Hauler.AI.Pathfinding;
using Hauler.World;

namespace Hauler.Entity.Enemy {
    [RequireComponent(typeof(Rigidbody2D))]
    public class TheArmBehavior : MonoBehaviour {
        [Range(0, 10)] public int difficulty = 3;

        [Space]
        public TargetWaypoint startingWaypoint;
        public TargetWaypoint[] ignoredWaypoints;
        public float speed = 6f, acceleration = 20f, rotationSpeed = 10f;

        [Space]
        public float maxSenseDistance = 12f;

        [Space]
        public float pathAdvanceRadius = .5f;
        public float pathRefreshInterval = 12f;
        public float pathRefreshDiff = 6f;
        public Vector2 pathRefreshChance = new Vector2(0f, 1f);

        [Space]
        public float footstepInterval = .3f;

        protected Rigidbody2D rb;
        protected List<TargetWaypoint> ignoredWaypointsList;
        protected BaseAudioController footstepAudioController;
        protected TargetWaypoint currentNode, previousNode;
        protected Vector2[] path;
        protected int targetIndex;

        protected float lastRequestAttemptTime;
        protected float footstepTime, lastFootstepTime;
        protected bool pathCompleted;

        protected Coroutine findNextWaypoint, chasePerceivedTarget;

        public float DifficultyMult => difficulty / 10f;
        public bool Chasing { get; private set; }
        public Transform PrimaryTarget { get; private set; }
        public Vector2 PerceivedTargetPosition { get; private set; }
        protected virtual bool CanRefreshPath => Random.value < Mathf.Lerp(pathRefreshChance.x, pathRefreshChance.y, DifficultyMult);

        protected virtual void Awake() {
            rb = GetComponent<Rigidbody2D>();
            footstepAudioController = GetComponent<BaseAudioController>();
            ignoredWaypointsList = new List<TargetWaypoint>(ignoredWaypoints);

            PrimaryTarget = GameObject.FindWithTag("Player").transform;
            currentNode = startingWaypoint;
        }

        /*void Start() {
            if(enabled && gameObject.activeInHierarchy) StartCoroutine(RequestPath());
        }

        void OnEnable() {
            StartCoroutine(RequestPath());
        }

        void OnDisable() {
            StopAllCoroutines();
        }*/

        protected virtual void Update() {
            // Request a new path if this seeker has no path or the current path has been traversed
            if(!Chasing && CanRefreshPath && (path == null || path?.Length == 0 || pathCompleted)) {
                if(Time.time > lastRequestAttemptTime + pathRefreshInterval - pathRefreshDiff * DifficultyMult) {
                    TargetWaypoint next;
                    do {
                        next = currentNode.Select();
                    } while(ignoredWaypointsList.Contains(next));
                    RequestWanderPath(next);
                    lastRequestAttemptTime = Time.time;
                }
            }
            // Traverse the current path
            /*if(path != null) {
                if(targetIndex < path.Length) StartCoroutine(FindNextWaypoint());
            }*/
            // Trigger footstep sound
            if(rb.velocity.sqrMagnitude > .1f) {
                footstepTime += Time.deltaTime;
                if(footstepTime >= lastFootstepTime + footstepInterval) {
                    footstepAudioController.PlaySound();
                    lastFootstepTime = footstepTime;
                }
            }
        }

        protected virtual void FixedUpdate() {
            Vector2 worldPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 targetMov = Vector2.zero;
            float targetRot = rb.rotation;

            // Move towards the next waypoint in the current path
            if(path != null && !pathCompleted && targetIndex < path.Length) {
                targetMov = path[targetIndex] - worldPos;
                Vector2 targetLook = targetMov;
                targetRot = Mathf.Rad2Deg * Mathf.Atan2(targetLook.y, targetLook.x) - 90f;
            }
            rb.velocity = Vector2.Lerp(rb.velocity, targetMov.normalized * speed, acceleration * Time.fixedDeltaTime);
            rb.rotation = Mathf.LerpAngle(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);

            // Start chasing if the player is in sight
            if(!Chasing && HasLOS(PrimaryTarget.position)) {
                Chasing = true;
                chasePerceivedTarget = StartCoroutine(DoChasePerceivedTarget());
            }
        }

        protected bool HasLOS(Vector2 target) {
            Vector2 worldPos = new Vector2(transform.position.x, transform.position.y);
            Vector2 dir = target - worldPos;
            bool withinDist = dir.sqrMagnitude < maxSenseDistance * maxSenseDistance;
            if(withinDist) {
                bool unobstructed = !Physics2D.Raycast(transform.position, dir.normalized, Mathf.Min(maxSenseDistance, dir.magnitude), LayerMask.GetMask("Obstacle"));
                return unobstructed;
            }
            return false;
        }

        protected virtual IEnumerator DoChasePerceivedTarget() {
            while(Chasing) {
                bool hasLOS = HasLOS(PrimaryTarget.position);
                if(hasLOS) PerceivedTargetPosition = PrimaryTarget.position;
                if(pathCompleted && !hasLOS) {
                    break;
                }
                RequestChasePath(PerceivedTargetPosition);
                yield return new WaitForSeconds(.1f);
            }
            StopChasing();
        }

        protected void StopChasing() {
            Chasing = false;
            currentNode = GameController.Instance.FindNearestWaypointInRoom(transform.position);
            if(chasePerceivedTarget != null) StopCoroutine(chasePerceivedTarget);
            pathCompleted = true;
        }

        protected virtual void RequestWanderPath(TargetWaypoint pathTarget) {
            Chasing = false;
            previousNode = currentNode;
            currentNode = pathTarget;
            PathRequestManager.RequestPath(transform.position, pathTarget.transform.position, OnWanderPathFound);
        }

        protected virtual void RequestChasePath(Vector2 worldTarget) {
            Chasing = true;
            PathRequestManager.RequestPath(transform.position, worldTarget, OnChasePathFound);
        }

        protected virtual IEnumerator FindNextWaypoint() {
            if(!pathCompleted && targetIndex < path.Length) {
                Vector2 current = path[targetIndex];

                while(true) {
                    Vector2 worldPos = new Vector2(transform.position.x, transform.position.y);
                    Vector2 remainingDistance = current - worldPos;
                    if(remainingDistance.sqrMagnitude <= pathAdvanceRadius * pathAdvanceRadius) {
                        targetIndex++;
                        if(targetIndex >= path.Length) {
                            pathCompleted = true;
                            yield break;
                        }
                        current = path[targetIndex];
                    }
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        public virtual void OnWanderPathFound(Vector2[] newPath, bool success) {
            if(findNextWaypoint != null) StopCoroutine(findNextWaypoint);
            if(success) {
                path = newPath;
                targetIndex = 0;
                pathCompleted = false;
                findNextWaypoint = StartCoroutine(FindNextWaypoint());
            } else {
                currentNode = previousNode;
            }
        }

        public virtual void OnChasePathFound(Vector2[] newPath, bool success) {
            if(findNextWaypoint != null) StopCoroutine(findNextWaypoint);
            if(success) {
                path = newPath;
                targetIndex = 0;
                pathCompleted = false;
                findNextWaypoint = StartCoroutine(FindNextWaypoint());
            } else {
                StopChasing();
            }
        }

        protected virtual void OnDrawGizmos() {
            if(path != null && path.Length > 0 && !pathCompleted) {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, path[targetIndex]);
                for(int i = targetIndex + 1; i < path.Length; i++) {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            } else {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, .5f);
            }

            /*if(currentNode != null) {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(currentNode.transform.position, .5f);
            }*/

            if(PrimaryTarget != null) {
                Gizmos.color = Color.blue;
                if(Chasing) Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, (PrimaryTarget.position - transform.position).normalized * maxSenseDistance);
            }
        }
    }
}
