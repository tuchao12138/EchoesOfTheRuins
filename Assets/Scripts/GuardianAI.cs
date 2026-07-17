using UnityEngine;
using UnityEngine.AI;

namespace EchoesOfTheRuins
{
    public sealed class GuardianAI : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private Transform player;
        [SerializeField, Min(0.1f)] private float patrolSpeed = 2f;
        [SerializeField, Min(0.1f)] private float chaseSpeed = 4f;
        [SerializeField, Min(0.1f)] private float detectionRange = 8f;
        [SerializeField, Min(0.1f)] private float captureRange = 1.25f;
        [SerializeField, Min(0.1f)] private float captureCooldown = 1f;
        [SerializeField, Min(0.05f)] private float waypointReachDistance = .4f;

        public bool IsChasing { get; private set; }
        private NavMeshAgent agent;
        private int waypointIndex;
        private float nextCaptureTime;

        public void Configure(Transform targetPlayer, Transform[] patrolWaypoints)
        {
            player = targetPlayer;
            waypoints = patrolWaypoints;
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            ResolvePlayer();
        }

        private void Update()
        {
            ResolvePlayer();
            if (player == null) return;
            float distance = Vector3.Distance(transform.position, player.position);
            IsChasing = distance <= detectionRange;
            if (distance <= captureRange && Time.time >= nextCaptureTime)
            {
                GameManager.Instance?.ResetPlayerToCheckpoint();
                nextCaptureTime = Time.time + captureCooldown;
                IsChasing = false;
                return;
            }

            Vector3 destination = IsChasing ? player.position : GetPatrolDestination();
            MoveTo(destination, IsChasing ? chaseSpeed : patrolSpeed);
        }

        private void ResolvePlayer()
        {
            if (player != null) return;
            if (GameManager.Instance != null) player = GameManager.Instance.PlayerTransform;
            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null) player = playerObject.transform;
            }
        }

        private Vector3 GetPatrolDestination()
        {
            if (waypoints == null || waypoints.Length == 0) return transform.position;
            Transform waypoint = waypoints[waypointIndex];
            if (waypoint != null && Vector3.Distance(transform.position, waypoint.position) <= waypointReachDistance)
                waypointIndex = (waypointIndex + 1) % waypoints.Length;
            return waypoints[waypointIndex] != null ? waypoints[waypointIndex].position : transform.position;
        }

        private void MoveTo(Vector3 destination, float speed)
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.speed = speed;
                agent.SetDestination(destination);
                return;
            }

            Vector3 flatDestination = new Vector3(destination.x, transform.position.y, destination.z);
            Vector3 direction = flatDestination - transform.position;
            if (direction.sqrMagnitude > .001f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);
                transform.position = Vector3.MoveTowards(transform.position, flatDestination, speed * Time.deltaTime);
            }
        }
    }
}
