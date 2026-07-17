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
        [SerializeField, Range(10f, 180f)] private float fieldOfView = 100f;
        [SerializeField, Min(0.1f)] private float captureRange = 1.25f;
        [SerializeField, Min(0.1f)] private float captureCooldown = 1f;
        [SerializeField, Min(0.05f)] private float waypointReachDistance = .4f;

        public bool IsChasing => CurrentState == GuardianState.Chase;
        public GuardianState CurrentState { get; private set; } = GuardianState.Patrol;
        private NavMeshAgent agent;
        private int waypointIndex;
        private float nextCaptureTime;
        private GuardianBrain brain;
        private Vector3 investigationPoint;
        private bool hasInvestigationPoint;
        private bool receivedNoise;

        public void Configure(Transform targetPlayer, Transform[] patrolWaypoints)
        {
            player = targetPlayer;
            waypoints = patrolWaypoints;
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            brain = new GuardianBrain();
            ResolvePlayer();
        }

        private void OnEnable() => NoiseSystem.NoiseCreated += OnNoiseCreated;
        private void OnDisable() => NoiseSystem.NoiseCreated -= OnNoiseCreated;

        private void Update()
        {
            ResolvePlayer();
            if (player == null) return;
            float distance = Vector3.Distance(transform.position, player.position);
            bool seesPlayer = CanSeePlayer(distance);
            bool capture = distance <= captureRange && seesPlayer && Time.time >= nextCaptureTime;
            CurrentState = brain.Tick(Time.deltaTime, new GuardianPerception(seesPlayer, receivedNoise, capture));
            receivedNoise = false;
            if (CurrentState == GuardianState.Capture)
            {
                GameManager.Instance?.ResetPlayerToCheckpoint();
                nextCaptureTime = Time.time + captureCooldown;
                return;
            }

            Vector3 destination = CurrentState == GuardianState.Chase ? player.position :
                (CurrentState == GuardianState.Investigate || CurrentState == GuardianState.Search) && hasInvestigationPoint ? investigationPoint : GetPatrolDestination();
            MoveTo(destination, CurrentState == GuardianState.Chase ? chaseSpeed : patrolSpeed);
        }

        private bool CanSeePlayer(float distance)
        {
            var stealth = player.GetComponent<PlayerController>();
            float effectiveRange = detectionRange * (stealth != null ? stealth.VisibilityMultiplier : 1f);
            if (distance > effectiveRange) return false;
            Vector3 origin = transform.position + Vector3.up * 1.2f;
            Vector3 target = player.position + Vector3.up * 1f;
            Vector3 direction = target - origin;
            if (Vector3.Angle(transform.forward, direction) > fieldOfView * .5f) return false;
            if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, direction.magnitude, ~0, QueryTriggerInteraction.Ignore))
                return hit.transform == player || hit.transform.IsChildOf(player);
            return false;
        }

        private void OnNoiseCreated(Vector3 position, float radius)
        {
            if (Vector3.Distance(transform.position, position) > radius) return;
            investigationPoint = position;
            hasInvestigationPoint = true;
            receivedNoise = true;
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
