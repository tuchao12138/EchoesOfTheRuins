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
        [SerializeField] private LayerMask obstacleMask = ~0;

        public bool IsChasing => CurrentState == GuardianState.Chase;
        public GuardianState CurrentState { get; private set; } = GuardianState.Patrol;
        public float AlertLevel { get; private set; }
        public GuardianAttackPhase AttackPhase { get; private set; } = GuardianAttackPhase.None;
        public bool NavigationReady => agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh;
        public event System.Action<GuardianState, float> AlertStateChanged;
        public event System.Action<string, GuardianAttackPhase> GuardianAttackChanged;
        public event System.Action<string> PlayerStruck;
        private NavMeshAgent agent;
        private int waypointIndex;
        private float nextCaptureTime;
        private GuardianBrain brain;
        private Vector3 investigationPoint;
        private bool hasInvestigationPoint;
        private bool receivedNoise;
        private CharacterMotionAnimator motionAnimator;
        private GuardianAttackSequence attackSequence;
        private GuardianVisionCone visionCone;
        private bool hitResponseRegistered;

        public void Configure(Transform targetPlayer, Transform[] patrolWaypoints)
        {
            player = targetPlayer;
            waypoints = patrolWaypoints;
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            brain = new GuardianBrain();
            attackSequence = new GuardianAttackSequence();
            visionCone = GetComponent<GuardianVisionCone>();
            if (visionCone == null) visionCone = gameObject.AddComponent<GuardianVisionCone>();
            visionCone.Configure(detectionRange, fieldOfView, obstacleMask);
            visionCone.SetState(CurrentState, AlertLevel);
            ResolvePlayer();
        }

        private void OnEnable() => NoiseSystem.NoiseCreated += OnNoiseCreated;
        private void OnDisable() => NoiseSystem.NoiseCreated -= OnNoiseCreated;

        private void Update()
        {
            ResolvePlayer();
            if (player == null) return;
            if (AttackPhase != GuardianAttackPhase.None)
            {
                TickAttack();
                return;
            }
            float distance = Vector3.Distance(transform.position, player.position);
            bool seesPlayer = (TutorialDirector.Active == null || TutorialDirector.Active.CanBeDetected) && CanSeePlayer(distance);
            bool capture = distance <= captureRange && seesPlayer && Time.time >= nextCaptureTime;
            SetState(brain.Tick(Time.deltaTime, new GuardianPerception(seesPlayer, receivedNoise, capture)));
            if (motionAnimator == null) motionAnimator = GetComponent<CharacterMotionAnimator>();
            motionAnimator?.Play(ToAnimationRole(CurrentState));
            receivedNoise = false;
            if (CurrentState == GuardianState.AttackTelegraph || CurrentState == GuardianState.Capture)
            {
                BeginAttack(player);
                return;
            }

            Vector3 destination = CurrentState == GuardianState.Chase ? player.position :
                (CurrentState == GuardianState.Investigate || CurrentState == GuardianState.Search) && hasInvestigationPoint ? investigationPoint : GetPatrolDestination();
            MoveTo(destination, CurrentState == GuardianState.Chase ? chaseSpeed : patrolSpeed);
        }

        public void DebugBeginAttack(Transform target) => BeginAttack(target);

        private void BeginAttack(Transform target)
        {
            if (target == null || Time.time < nextCaptureTime || AttackPhase != GuardianAttackPhase.None) return;
            player = target;
            attackSequence.Begin();
            StopNavigation();
            SetAttackPhase(GuardianAttackPhase.Telegraph);
            motionAnimator?.Play(AnimationRole.Attack, restart: true);
        }

        private void TickAttack()
        {
            StopNavigation();
            RotateTowardPlayer();
            bool targetInRange = Vector3.Distance(transform.position, player.position) <= 1.7f;
            AttackFrame frame = attackSequence.Tick(Time.deltaTime, targetInRange);
            SetAttackPhase(frame.Phase);
            if (frame.ShouldQueryHit && HasClearMeleePath())
            {
                attackSequence.ConfirmHit();
                SetAttackPhase(GuardianAttackPhase.HitConfirmed);
                PlayerStruck?.Invoke(gameObject.name);
            }
            if (!frame.SequenceFinished) return;
            nextCaptureTime = Time.time + captureCooldown;
            SetAttackPhase(GuardianAttackPhase.None);
            if (NavigationReady) agent.isStopped = false;
        }

        private bool HasClearMeleePath()
        {
            Vector3 origin = transform.position + Vector3.up;
            Vector3 target = player.position + Vector3.up;
            Vector3 delta = target - origin;
            if (delta.magnitude > 1.7f) return false;
            if (!Physics.Raycast(origin, delta.normalized, out RaycastHit hit, delta.magnitude,
                    obstacleMask, QueryTriggerInteraction.Ignore)) return true;
            return hit.transform == player || hit.transform.IsChildOf(player);
        }

        private void StopNavigation()
        {
            if (!NavigationReady) return;
            agent.isStopped = true;
            agent.ResetPath();
        }

        private void RotateTowardPlayer()
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= .001f) return;
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(direction),
                540f * Time.deltaTime);
        }

        private void SetAttackPhase(GuardianAttackPhase phase)
        {
            if (AttackPhase == phase) return;
            AttackPhase = phase;
            GuardianAttackChanged?.Invoke(gameObject.name, phase);
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

        private void SetState(GuardianState nextState)
        {
            float nextAlert = nextState == GuardianState.Chase || nextState == GuardianState.Capture ? 1f :
                nextState == GuardianState.Investigate || nextState == GuardianState.Search ? .55f : 0f;
            if (CurrentState == nextState && Mathf.Approximately(AlertLevel, nextAlert)) return;
            CurrentState = nextState;
            AlertLevel = nextAlert;
            visionCone?.SetState(CurrentState, AlertLevel);
            AlertStateChanged?.Invoke(CurrentState, AlertLevel);
        }

        private static AnimationRole ToAnimationRole(GuardianState state) => state switch
        {
            GuardianState.Patrol => AnimationRole.Walk,
            GuardianState.Investigate => AnimationRole.Walk,
            GuardianState.Search => AnimationRole.Walk,
            GuardianState.Chase => AnimationRole.Run,
            GuardianState.AttackTelegraph => AnimationRole.Attack,
            GuardianState.Strike => AnimationRole.Attack,
            GuardianState.Recovery => AnimationRole.Attack,
            GuardianState.Capture => AnimationRole.Attack,
            _ => AnimationRole.Idle
        };

        private void ResolvePlayer()
        {
            if (player == null && GameManager.Instance != null) player = GameManager.Instance.PlayerTransform;
            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null) player = playerObject.transform;
            }
            EnsureHitResponse();
        }

        private void EnsureHitResponse()
        {
            if (hitResponseRegistered || player == null || GameManager.Instance == null) return;
            PlayerHitResponse response = player.GetComponent<PlayerHitResponse>();
            if (response == null) response = player.gameObject.AddComponent<PlayerHitResponse>();
            response.Configure(player.GetComponent<PlayerController>(), GameManager.Instance);
            response.TrackGuardian(this);
            hitResponseRegistered = true;
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
