using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using Hauler.World;
using Hauler.Entity.Enemy;

namespace Hauler {
    public class GameController : MonoBehaviour {
        public Transform player;
        public float playerRoomCheckInterval = .1f;
        public LayerMask roomBoundaryMask;

        [Space]
        public AudioSource safeBGMSource;
        public AudioSource chaseBGMSource;
        public float safeBGMVolume = .1f, chaseBGMVolume = .25f;
        public float BGMRisingEdge = .08f, BGMFallingEdge = .04f;

        [Space]
        public TheArmBehavior theArm;

        public RoomBoundary PlayerRoom { get; private set; }

        public static GameController Instance { get; private set; }

        bool wasChasingLastFrame;

        void Awake() {
            Instance = this;

            if(player == null) player = GameObject.FindWithTag("Player")?.transform;
            StartCoroutine(PlayerRoomCheck());

            safeBGMSource.volume = safeBGMVolume;
            chaseBGMSource.volume = 0f;
        }

        void Update() {
            float targetSafeBGMVol = safeBGMVolume, targetChaseBGMVol = 0f, transition = BGMFallingEdge;
            if(theArm.Chasing) {
                targetSafeBGMVol = 0f;
                targetChaseBGMVol = chaseBGMVolume;
                transition = BGMRisingEdge;
            }
            safeBGMSource.volume = Mathf.MoveTowards(safeBGMSource.volume, targetSafeBGMVol, transition * Time.deltaTime);
            chaseBGMSource.volume = Mathf.MoveTowards(chaseBGMSource.volume, targetChaseBGMVol, transition * Time.deltaTime);
            wasChasingLastFrame = theArm.Chasing;
        }

        IEnumerator PlayerRoomCheck() {
            while(true) {
                PlayerRoom = RoomAt(player.position);
                yield return new WaitForSeconds(playerRoomCheckInterval);
            }
        }

        public TargetWaypoint FindNearestWaypointInRoom(Vector2 position) {
            RoomBoundary room = RoomAt(position);
            if(room?.waypointsInRoom?.Length > 0) {
                TargetWaypoint currentPoint = room.waypointsInRoom[0];
                for(int i = 1; i < room.waypointsInRoom.Length; i++) {
                    TargetWaypoint nextPoint = room.waypointsInRoom[i];
                    Vector2 currentPos = currentPoint.transform.position;
                    Vector2 nextPointPos = nextPoint.transform.position;
                    if((nextPointPos - position).sqrMagnitude < (currentPos - position).sqrMagnitude) {
                        currentPoint = nextPoint;
                    }
                }
                return currentPoint;
            }
            return null;
        }

        public RoomBoundary RoomAt(Vector2 position) {
            Collider2D boundary = Physics2D.OverlapPoint(position, roomBoundaryMask);
            return boundary?.GetComponent<RoomBoundary>();
        }
    }
}
