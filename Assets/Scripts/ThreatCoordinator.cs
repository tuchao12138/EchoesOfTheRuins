using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfTheRuins
{
    /// <summary>Aggregates every guardian into one stable, player-facing threat signal.</summary>
    public sealed class ThreatCoordinator : MonoBehaviour
    {
        [SerializeField] private GuardianAI[] guardians;
        [SerializeField] private Transform player;
        private readonly ThreatModel model = new ThreatModel();
        private readonly Dictionary<GuardianAI, Action<GuardianState, float>> alertHandlers = new Dictionary<GuardianAI, Action<GuardianState, float>>();
        private readonly Dictionary<GuardianAI, Action<string, GuardianAttackPhase>> attackHandlers = new Dictionary<GuardianAI, Action<string, GuardianAttackPhase>>();
        private readonly Dictionary<GuardianAI, string> guardianIds = new Dictionary<GuardianAI, string>();

        public ThreatSnapshot Current => model.Current;
        public event Action<ThreatSnapshot> Changed;

        private void Awake()
        {
            model.Changed += snapshot => Changed?.Invoke(snapshot);
        }

        private void OnEnable()
        {
            RefreshGuardians();
        }

        private void LateUpdate()
        {
            RefreshGuardians();
            foreach (GuardianAI guardian in new List<GuardianAI>(alertHandlers.Keys))
                if (guardian == null || !guardian.isActiveAndEnabled) Unsubscribe(guardian);
        }

        public void Register(GuardianAI guardian) => Subscribe(guardian);
        public void Unregister(GuardianAI guardian) => Unsubscribe(guardian);

        private void RefreshGuardians()
        {
            if (guardians == null || guardians.Length == 0) guardians = FindObjectsByType<GuardianAI>(FindObjectsSortMode.None);
            if (player == null && GameManager.Instance != null) player = GameManager.Instance.PlayerTransform;
            foreach (GuardianAI guardian in FindObjectsByType<GuardianAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) Subscribe(guardian);
        }

        private void Subscribe(GuardianAI guardian)
        {
            if (guardian == null || alertHandlers.ContainsKey(guardian)) return;
            Action<GuardianState, float> alert = (_, __) => Publish(guardian);
            Action<string, GuardianAttackPhase> attack = (_, __) => Publish(guardian);
            alertHandlers.Add(guardian, alert);
            attackHandlers.Add(guardian, attack);
            guardianIds.Add(guardian, guardian.gameObject.name);
            guardian.AlertStateChanged += alert;
            guardian.GuardianAttackChanged += attack;
            Publish(guardian);
        }

        private void Publish(GuardianAI guardian)
        {
            if (guardian == null) return;
            if (player == null && GameManager.Instance != null) player = GameManager.Instance.PlayerTransform;
            Vector3 direction = player == null ? guardian.transform.forward : player.InverseTransformDirection(guardian.transform.position - player.position);
            model.Update(guardian.gameObject.name, guardian.CurrentState, guardian.AlertLevel, guardian.AttackPhase, direction);
        }

        private void Unsubscribe(GuardianAI guardian)
        {
            if (!guardianIds.TryGetValue(guardian, out string guardianId)) return;
            if (guardian != null && alertHandlers.TryGetValue(guardian, out Action<GuardianState, float> alert)) guardian.AlertStateChanged -= alert;
            if (guardian != null && attackHandlers.TryGetValue(guardian, out Action<string, GuardianAttackPhase> attack)) guardian.GuardianAttackChanged -= attack;
            alertHandlers.Remove(guardian);
            attackHandlers.Remove(guardian);
            guardianIds.Remove(guardian);
            model.Remove(guardianId);
        }

        private void OnDestroy()
        {
            foreach (GuardianAI guardian in new List<GuardianAI>(alertHandlers.Keys)) Unsubscribe(guardian);
        }
    }
}
